import 'dart:convert';
import 'package:flutter/material.dart';
import 'package:scoped_model/scoped_model.dart';
import 'package:shared_preferences/shared_preferences.dart';
import '../services/auth_service.dart';

class UserModel extends Model {
  static const String _prefUserKey = 'saved_user_data';

  Map<String, dynamic> userdata = {};
  bool isLoading = false;
  String? lastErrorMessage;

  static UserModel of(BuildContext context) =>
      ScopedModel.of<UserModel>(context);

  bool isLoggedIn() {
    return userdata.isNotEmpty && userdata['id'] != null;
  }

  int? get idCliente {
    if (!isLoggedIn()) return null;
    return int.tryParse(userdata['id'].toString());
  }

  Future<void> loadCurrentUser() async {
    try {
      final prefs = await SharedPreferences.getInstance();
      final savedJson = prefs.getString(_prefUserKey);
      if (savedJson != null && savedJson.isNotEmpty) {
        userdata = Map<String, dynamic>.from(jsonDecode(savedJson) as Map);
        notifyListeners();
      }
    } catch (_) {
      userdata = {};
    }
  }

  Future<void> signUp({
    required Map<String, dynamic> userData,
    required String pass,
    required VoidCallback onSucess,
    required Function(String error) onFail,
  }) async {
    isLoading = true;
    lastErrorMessage = null;
    notifyListeners();

    try {
      final nome = (userData['name'] ?? '').toString().trim();
      final email = (userData['email'] ?? '').toString().trim().toLowerCase();
      final telefone = (userData['phone'] ?? userData['telefone'] ?? '').toString().trim();
      final endereco = (userData['address'] ?? '').toString().trim();

      if (nome.isEmpty || email.isEmpty || !email.contains('@') || pass.length < 6) {
        lastErrorMessage = 'Preencha os campos obrigatorios (senha minima de 6 caracteres).';
        onFail(lastErrorMessage!);
        return;
      }

      final response = await AuthService.register(
        nome: nome,
        email: email,
        senha: pass,
        telefone: telefone,
      );

      if (response.success && response.data != null) {
        final idCriado = response.data!['id'];

        // Efetua login automatico em seguida
        final loginResp = await AuthService.login(email, pass);
        if (loginResp.success && loginResp.data != null) {
          await _saveSession(loginResp.data!, endereco: endereco);
        } else {
          userdata = {
            'id': idCriado,
            'name': nome,
            'email': email,
            'address': endereco,
            'phone': telefone,
            'isAdmin': false,
          };
          final prefs = await SharedPreferences.getInstance();
          await prefs.setString(_prefUserKey, jsonEncode(userdata));
        }

        onSucess();
      } else {
        lastErrorMessage = response.message ?? 'Falha ao registrar cliente.';
        onFail(lastErrorMessage!);
      }
    } catch (e) {
      lastErrorMessage = 'Erro ao conectar ao servidor: $e';
      onFail(lastErrorMessage!);
    } finally {
      isLoading = false;
      notifyListeners();
    }
  }

  Future<void> signIn({
    required String email,
    required String pass,
    required VoidCallback onSucess,
    required Function(String error) onFail,
  }) async {
    isLoading = true;
    lastErrorMessage = null;
    notifyListeners();

    try {
      final response = await AuthService.login(email, pass);

      if (response.success && response.data != null) {
        await _saveSession(response.data!);
        onSucess();
      } else {
        lastErrorMessage = response.message ?? 'E-mail ou senha invalidos.';
        onFail(lastErrorMessage!);
      }
    } catch (e) {
      lastErrorMessage = 'Erro ao comunicar com a API: $e';
      onFail(lastErrorMessage!);
    } finally {
      isLoading = false;
      notifyListeners();
    }
  }

  Future<void> _saveSession(Map<String, dynamic> data, {String? endereco}) async {
    userdata = {
      'id': data['id'],
      'name': data['nome'] ?? data['name'] ?? '',
      'email': data['email'] ?? '',
      'isAdmin': data['isAdmin'] == true,
      'address': endereco ?? userdata['address'] ?? '',
    };
    final prefs = await SharedPreferences.getInstance();
    await prefs.setString(_prefUserKey, jsonEncode(userdata));
  }

  Future<void> signOut() async {
    userdata = {};
    final prefs = await SharedPreferences.getInstance();
    await prefs.remove(_prefUserKey);
    notifyListeners();
  }

  Future<bool> recoverPass(String email) async {
    // Validacao formal de presenca de email
    return email.trim().contains('@');
  }
}
