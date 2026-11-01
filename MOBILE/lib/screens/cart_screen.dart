import 'package:apppim/screens/order_screen.dart';
import 'package:flutter/material.dart';
import 'package:scoped_model/scoped_model.dart';
import 'package:apppim/models/user_model.dart';
import 'package:apppim/models/cart_model.dart';
import 'package:apppim/screens/login_screen.dart';
import 'package:apppim/tiles/cart_tile.dart';
import 'package:apppim/widgets/discount_card.dart';
import 'package:apppim/widgets/ship_card.dart';
import 'package:apppim/widgets/cart_price.dart';

class CartScreen extends StatelessWidget {
  const CartScreen({super.key});

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      appBar: AppBar(
        title: const Text('Meu Carrinho'),
        centerTitle: true,
        actions: [
          Container(
            padding: const EdgeInsets.only(right: 16.0),
            alignment: Alignment.center,
            child: ScopedModelDescendant<CartModel>(
              builder: (context, child, model) {
                final int p = model.itemCount;
                return Text(
                  '$p ${p == 1 ? "ITEM" : "ITENS"}',
                  style: const TextStyle(fontSize: 16.0, fontWeight: FontWeight.bold),
                );
              },
            ),
          ),
        ],
      ),
      body: ScopedModelDescendant<CartModel>(
        builder: (context, child, model) {
          final bool isLoggedIn = UserModel.of(context).isLoggedIn();

          if (model.isLoading) {
            return const Center(
              child: Column(
                mainAxisSize: MainAxisSize.min,
                children: [
                  CircularProgressIndicator(),
                  SizedBox(height: 16),
                  Text('Registrando pedido e processando estoque...'),
                ],
              ),
            );
          }

          if (!isLoggedIn) {
            return Center(
              child: Padding(
                padding: const EdgeInsets.all(24.0),
                child: Column(
                  mainAxisSize: MainAxisSize.min,
                  crossAxisAlignment: CrossAxisAlignment.stretch,
                  children: [
                    Icon(
                      Icons.remove_shopping_cart,
                      size: 80.0,
                      color: Theme.of(context).primaryColor,
                    ),
                    const SizedBox(height: 16.0),
                    const Text(
                      'Faca login para acessar seu carrinho e finalizar compras!',
                      style: TextStyle(
                        fontSize: 18.0,
                        fontWeight: FontWeight.bold,
                      ),
                      textAlign: TextAlign.center,
                    ),
                    const SizedBox(height: 16.0),
                    ElevatedButton(
                      style: ElevatedButton.styleFrom(
                        foregroundColor: Colors.white,
                        backgroundColor: Theme.of(context).primaryColor,
                      ),
                      onPressed: () {
                        Navigator.of(context).push(
                          MaterialPageRoute(
                            builder: (_) => LoginScreen(),
                          ),
                        );
                      },
                      child: const Text(
                        'Entrar na Minha Conta',
                        style: TextStyle(fontSize: 16.0),
                      ),
                    ),
                  ],
                ),
              ),
            );
          }

          if (model.products.isEmpty) {
            return Center(
              child: Padding(
                padding: const EdgeInsets.all(24.0),
                child: Column(
                  mainAxisSize: MainAxisSize.min,
                  children: [
                    Icon(
                      Icons.shopping_bag_outlined,
                      size: 70,
                      color: Colors.grey.shade400,
                    ),
                    const SizedBox(height: 16),
                    const Text(
                      'Seu carrinho esta vazio!',
                      style: TextStyle(
                        fontSize: 18.0,
                        fontWeight: FontWeight.bold,
                      ),
                      textAlign: TextAlign.center,
                    ),
                    const SizedBox(height: 8),
                    const Text(
                      'Explore nosso catalogo e adicione instrumentos musicais.',
                      style: TextStyle(color: Colors.grey),
                      textAlign: TextAlign.center,
                    ),
                  ],
                ),
              ),
            );
          }

          return ListView(
            padding: const EdgeInsets.only(top: 4.0, bottom: 20.0),
            children: [
              ...model.products.map((product) => CartTile(product)),
              const SizedBox(height: 8.0),
              const DiscountCard(),
              const ShipCard(),
              const SizedBox(height: 8.0),
              CartPrice(() async {
                final String? orderId = await model.finishOrder();

                if (!context.mounted) return;

                if (orderId != null && orderId.isNotEmpty) {
                  Navigator.of(context).pushReplacement(
                    MaterialPageRoute(
                      builder: (_) => OrderScreen(orderId),
                    ),
                  );
                } else {
                  ScaffoldMessenger.of(context).showSnackBar(
                    SnackBar(
                      content: Text(
                        model.lastOrderError ?? 'Nao foi possivel concluir a compra.',
                      ),
                      backgroundColor: Colors.redAccent,
                      duration: const Duration(seconds: 4),
                    ),
                  );
                }
              }),
            ],
          );
        },
      ),
    );
  }
}
