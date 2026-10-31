
import 'package:flutter/material.dart';
import 'package:scoped_model/scoped_model.dart';

class UserModel extends Model {
  static final Map<String, Map<String, dynamic>> _users = {};

  Map<String, dynamic> userdata = {};

  bool isLoading = false;

  static UserModel of(BuildContext context) =>
      ScopedModel.of<UserModel>(context);

  bool isLoggedIn() {
    return userdata.isNotEmpty;
  }

  Future<void> signUp({
    required Map<String, dynamic> userData,
    required String pass,
    required VoidCallback onSucess,
    required VoidCallback onFail,
  }) async {
    isLoading = true;
    notifyListeners();

    try {
      final email = userData["email"]
          .toString()
          .trim()
          .toLowerCase();

      if (email.isEmpty ||
          !email.contains("@") ||
          pass.length < 6 ||
          userData["name"] == null ||
          userData["address"] == null) {
        onFail();
        return;
      }

      if (_users.containsKey(email)) {
        onFail();
        return;
      }

      _users[email] = {
        "name": userData["name"],
        "email": email,
        "address": userData["address"],
        "password": pass,
      };

      userdata = {
        "name": userData["name"],
        "email": email,
        "address": userData["address"],
      };

      onSucess();
    } catch (e) {
      onFail();
    } finally {
      isLoading = false;
      notifyListeners();
    }
  }

  Future<void> signIn({
    required String email,
    required String pass,
    required VoidCallback onSucess,
    required VoidCallback onFail,
  }) async {
    isLoading = true;
    notifyListeners();

    try {
      final emailNormalizado = email.trim().toLowerCase();
      final user = _users[emailNormalizado];

      if (user != null && user["password"] == pass) {
        userdata = {
          "name": user["name"],
          "email": user["email"],
          "address": user["address"],
        };

        onSucess();
      } else {
        onFail();
      }
    } catch (e) {
      onFail();
    } finally {
      isLoading = false;
      notifyListeners();
    }
  }

  Future<void> signOut() async {
    userdata = {};
    notifyListeners();
  }

  Future<bool> recoverPass(String email) async {
    return _users.containsKey(email.trim().toLowerCase());
  }

  void clearLocalUsers() {
    _users.clear();
    userdata = {};
    notifyListeners();
  }
}
