import 'package:flutter/material.dart';
import 'package:scoped_model/scoped_model.dart';
import 'package:apppim/models/cart_model.dart';
import 'package:apppim/models/user_model.dart';
import 'package:apppim/screens/home_screen.dart';
import 'package:apppim/services/api_config.dart';

void main() async {
  WidgetsFlutterBinding.ensureInitialized();
  await ApiConfig.init();

  final userModel = UserModel();
  await userModel.loadCurrentUser();

  runApp(MyApp(userModel: userModel));
}

class MyApp extends StatelessWidget {
  final UserModel userModel;

  const MyApp({super.key, required this.userModel});

  @override
  Widget build(BuildContext context) {
    return ScopedModel<UserModel>(
      model: userModel,
      child: ScopedModelDescendant<UserModel>(
        builder: (context, child, model) {
          return ScopedModel<CartModel>(
            model: CartModel(model),
            child: MaterialApp(
              title: 'ShopNow',
              theme: ThemeData(
                primarySwatch: Colors.blue,
                primaryColor: const Color.fromARGB(255, 4, 125, 141),
                colorScheme: ColorScheme.fromSwatch().copyWith(
                  primary: const Color.fromARGB(255, 4, 125, 141),
                  secondary: const Color.fromARGB(255, 4, 125, 141),
                ),
              ),
              debugShowCheckedModeBanner: false,
              home: HomeScreen(),
            ),
          );
        },
      ),
    );
  }
}
