
import 'package:flutter/material.dart';
import 'package:scoped_model/scoped_model.dart';
import 'package:apppim/models/cart_model.dart';
import 'package:apppim/models/user_model.dart';
import 'package:apppim/screens/home_screen.dart';

void main() {
  runApp(const MyApp());
}

class MyApp extends StatelessWidget {
  const MyApp({super.key});

  @override
  Widget build(BuildContext context) {
    return ScopedModel<UserModel>(
      model: UserModel(),
      child: ScopedModel<CartModel>(
        model: CartModel(),
        child: MaterialApp(
          title: 'Loja Musical',
          theme: ThemeData(
            primarySwatch: Colors.blue,
            primaryColor: const Color.fromARGB(255, 4, 125, 141),
          ),
          debugShowCheckedModeBanner: false,
          home: HomeScreen(),
        ),
      ),
    );
  }
}
