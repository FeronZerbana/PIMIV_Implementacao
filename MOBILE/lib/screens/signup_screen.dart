import 'package:flutter/material.dart';
import 'package:scoped_model/scoped_model.dart';
import '../models/user_model.dart';
import '../screens/login_screen.dart';
import '../widgets/server_settings_dialog.dart';

class SignupScreen extends StatefulWidget {
  const SignupScreen({super.key});

  @override
  State<SignupScreen> createState() => _SignupScreenState();
}

class _SignupScreenState extends State<SignupScreen> {
  final _formKey = GlobalKey<FormState>();

  final _nameController = TextEditingController();
  final _emailController = TextEditingController();
  final _phoneController = TextEditingController();
  final _passController = TextEditingController();
  final _addressController = TextEditingController();

  @override
  void dispose() {
    _nameController.dispose();
    _emailController.dispose();
    _phoneController.dispose();
    _passController.dispose();
    _addressController.dispose();
    super.dispose();
  }

  void _onSuccess() {
    ScaffoldMessenger.of(context).showSnackBar(
      SnackBar(
        content: const Text("Conta criada e autenticada com sucesso!"),
        backgroundColor: Theme.of(context).primaryColor,
        duration: const Duration(seconds: 2),
      ),
    );

    Future.delayed(const Duration(milliseconds: 800), () {
      if (mounted) {
        Navigator.of(context).pop();
      }
    });
  }

  void _onFail(String error) {
    ScaffoldMessenger.of(context).showSnackBar(
      SnackBar(
        content: Text(error),
        backgroundColor: Colors.redAccent,
        duration: const Duration(seconds: 3),
      ),
    );
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      appBar: AppBar(
        title: const Text("Criar Conta"),
        centerTitle: true,
        actions: [
          IconButton(
            icon: const Icon(Icons.dns, size: 22),
            tooltip: 'Configurar Servidor',
            onPressed: () {
              ServerSettingsDialog.show(context);
            },
          ),
        ],
      ),
      body: ScopedModelDescendant<UserModel>(
        builder: (context, child, model) {
          return Form(
            key: _formKey,
            child: ListView(
              padding: const EdgeInsets.all(16.0),
              children: <Widget>[
                TextFormField(
                  controller: _nameController,
                  decoration: const InputDecoration(
                    border: OutlineInputBorder(),
                    labelText: "Nome Completo",
                    hintText: "Ex: Joao da Silva",
                  ),
                  validator: (text) {
                    if (text == null || text.trim().isEmpty) {
                      return "Informe seu nome completo";
                    }
                    return null;
                  },
                ),
                const SizedBox(height: 16.0),
                TextFormField(
                  controller: _emailController,
                  decoration: const InputDecoration(
                    border: OutlineInputBorder(),
                    labelText: "E-mail",
                    hintText: "exemplo@email.com",
                  ),
                  keyboardType: TextInputType.emailAddress,
                  validator: (text) {
                    if (text == null || text.isEmpty || !text.contains("@")) {
                      return "Informe um e-mail valido";
                    }
                    return null;
                  },
                ),
                const SizedBox(height: 16.0),
                TextFormField(
                  controller: _phoneController,
                  decoration: const InputDecoration(
                    border: OutlineInputBorder(),
                    labelText: "Telefone / Celular (Opcional)",
                    hintText: "(11) 98765-4321",
                  ),
                  keyboardType: TextInputType.phone,
                ),
                const SizedBox(height: 16.0),
                TextFormField(
                  controller: _passController,
                  decoration: const InputDecoration(
                    border: OutlineInputBorder(),
                    labelText: "Senha de Acesso",
                    hintText: "Minimo 6 caracteres",
                  ),
                  obscureText: true,
                  validator: (text) {
                    if (text == null || text.length < 6) {
                      return "A senha deve ter no minimo 6 caracteres";
                    }
                    return null;
                  },
                ),
                const SizedBox(height: 16.0),
                TextFormField(
                  controller: _addressController,
                  decoration: const InputDecoration(
                    border: OutlineInputBorder(),
                    labelText: "Endereco Residencial (Opcional)",
                    hintText: "Rua, numero, bairro e cidade",
                  ),
                ),
                const SizedBox(height: 20.0),
                SizedBox(
                  height: 48.0,
                  child: ElevatedButton(
                    style: ElevatedButton.styleFrom(
                      foregroundColor: Colors.white,
                      backgroundColor: Theme.of(context).primaryColor,
                    ),
                    onPressed: model.isLoading
                        ? null
                        : () {
                      if (_formKey.currentState!.validate()) {
                        model.signUp(
                          userData: {
                            "name": _nameController.text.trim(),
                            "email": _emailController.text.trim(),
                            "phone": _phoneController.text.trim(),
                            "address": _addressController.text.trim(),
                          },
                          pass: _passController.text,
                          onSucess: _onSuccess,
                          onFail: (err) => _onFail(err),
                        );
                      }
                    },
                    child: model.isLoading
                        ? const SizedBox(
                      width: 20,
                      height: 20,
                      child: CircularProgressIndicator(
                        color: Colors.white,
                        strokeWidth: 2,
                      ),
                    )
                        : const Text(
                      "Criar Minha Conta",
                      style: TextStyle(
                        fontSize: 17.0,
                        fontWeight: FontWeight.bold,
                      ),
                    ),
                  ),
                ),
                const SizedBox(height: 16.0),
                Center(
                  child: TextButton(
                    onPressed: () {
                      Navigator.of(context).pushReplacement(
                        MaterialPageRoute(
                          builder: (context) => const LoginScreen(),
                        ),
                      );
                    },
                    child: Text(
                      "Ja possui uma conta? Faca Login",
                      style: TextStyle(
                        color: Theme.of(context).primaryColor,
                        fontWeight: FontWeight.bold,
                        fontSize: 15,
                      ),
                    ),
                  ),
                ),
              ],
            ),
          );
        },
      ),
    );
  }
}
