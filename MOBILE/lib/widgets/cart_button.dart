import 'package:apppim/screens/cart_screen.dart';
import 'package:flutter/material.dart';

class CartButton extends StatelessWidget {
  const CartButton({super.key});

  @override
  Widget build(BuildContext context) {
    return FloatingActionButton(
      backgroundColor: Theme.of(context).primaryColor,
      onPressed: () {
        Navigator.of(context).push(
          MaterialPageRoute(builder: (context) => const CartScreen()),
        );
      },
      child: const Icon(Icons.shopping_cart, color: Colors.white),
    );
  }
}
