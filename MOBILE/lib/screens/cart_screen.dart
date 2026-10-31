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
        actions: [
          Container(
            padding: const EdgeInsets.only(right: 8.0),
            alignment: Alignment.center,
            child: ScopedModelDescendant<CartModel>(
              builder: (context, child, model) {
                final int p = model.itemCount;

                return Text(
                  '$p ${p == 1 ? "ITEM" : "ITENS"}',
                  style: const TextStyle(fontSize: 17.0),
                );
              },
            ),
          ),
        ],
      ),

      body: ScopedModelDescendant<CartModel>(
        builder: (context, child, model) {
          final bool isLoggedIn =
          UserModel.of(context).isLoggedIn();

          // Carregamento
          if (model.isLoading && isLoggedIn) {
            return const Center(
              child: CircularProgressIndicator(),
            );
          }

          // Usuário não autenticado
          if (!isLoggedIn) {
            return Center(
              child: Padding(
                padding: const EdgeInsets.all(16.0),
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
                      'Faça o login para acessar seu carrinho!',
                      style: TextStyle(
                        fontSize: 20.0,
                        fontWeight: FontWeight.bold,
                      ),
                      textAlign: TextAlign.center,
                    ),

                    const SizedBox(height: 16.0),

                    ElevatedButton(
                      style: ElevatedButton.styleFrom(
                        foregroundColor: Colors.white,
                        backgroundColor:
                        Theme.of(context).primaryColor,
                      ),
                      onPressed: () {
                        Navigator.of(context).push(
                          MaterialPageRoute(
                            builder: (_) => LoginScreen(),
                          ),
                        );
                      },
                      child: const Text(
                        'Entrar',
                        style: TextStyle(fontSize: 18.0),
                      ),
                    ),
                  ],
                ),
              ),
            );
          }

          // Carrinho vazio
          if (model.products.isEmpty) {
            return const Center(
              child: Text(
                'Nenhum produto no carrinho!',
                style: TextStyle(
                  fontSize: 20.0,
                  fontWeight: FontWeight.bold,
                ),
                textAlign: TextAlign.center,
              ),
            );
          }

          // Carrinho com produtos
          return ListView(
            padding: const EdgeInsets.only(
              top: 4.0,
              bottom: 16.0,
            ),
            children: [
              // Produtos do carrinho
              ...model.products.map(
                    (product) => CartTile(product),
              ),

              const SizedBox(height: 8.0),

              // Cupom de desconto
              const DiscountCard(),

              // Calcular frete
              const ShipCard(),

              const SizedBox(height: 8.0),

              // Resumo do pedido e botão finalizar
              CartPrice(() async {
                final String? orderId =
                await model.finishOrder();

                if (orderId != null &&
                    orderId.isNotEmpty &&
                    context.mounted) {
                  Navigator.of(context).pushReplacement(
                    MaterialPageRoute(
                      builder: (_) => OrderScreen(orderId),
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