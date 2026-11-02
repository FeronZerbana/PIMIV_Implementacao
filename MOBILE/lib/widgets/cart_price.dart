import 'package:apppim/models/cart_model.dart';
import 'package:flutter/material.dart';
import 'package:scoped_model/scoped_model.dart';

class CartPrice extends StatelessWidget {
  final VoidCallback buy;

  const CartPrice(this.buy, {super.key});

  @override
  Widget build(BuildContext context) {
    return Card(
      margin: const EdgeInsets.symmetric(
        horizontal: 8.0,
        vertical: 4.0,
      ),
      child: Padding(
        padding: const EdgeInsets.all(16.0),
        child: ScopedModelDescendant<CartModel>(
          builder: (context, child, model) {
            final double price = model.getProductsPrice();
            final double discount = model.getDiscount();
            final double ship = model.getShipPrice();
            final double total = price + ship - discount;

            return Column(
              crossAxisAlignment: CrossAxisAlignment.stretch,
              children: [
                const Text(
                  'Resumo do Pedido',
                  textAlign: TextAlign.start,
                  style: TextStyle(
                    fontWeight: FontWeight.w500,
                    fontSize: 16.0,
                  ),
                ),

                const SizedBox(height: 12.0),

                // Subtotal
                Row(
                  mainAxisAlignment: MainAxisAlignment.spaceBetween,
                  children: [
                    const Text('Subtotal'),
                    Text(
                      'R\$ ${price.toStringAsFixed(2)}',
                    ),
                  ],
                ),

                const Divider(),

                // Desconto
                Row(
                  mainAxisAlignment: MainAxisAlignment.spaceBetween,
                  children: [
                    Text(model.discountPercentage > 0
                        ? 'Desconto (${model.discountPercentage}% OFF)'
                        : 'Desconto'),
                    Text(
                      '- R\$ ${discount.toStringAsFixed(2)}',
                      style: TextStyle(
                        color: discount > 0 ? Colors.green.shade700 : null,
                        fontWeight: discount > 0 ? FontWeight.bold : null,
                      ),
                    ),
                  ],
                ),

                const Divider(),

                // Entrega
                Row(
                  mainAxisAlignment: MainAxisAlignment.spaceBetween,
                  children: [
                    const Text('Entrega'),
                    Text(
                      'R\$ ${ship.toStringAsFixed(2)}',
                    ),
                  ],
                ),

                const Divider(),

                const SizedBox(height: 12.0),

                // Total
                Row(
                  mainAxisAlignment: MainAxisAlignment.spaceBetween,
                  children: [
                    const Text(
                      'Total',
                      style: TextStyle(
                        fontWeight: FontWeight.w500,
                        fontSize: 16.0,
                      ),
                    ),
                    Text(
                      'R\$ ${total.toStringAsFixed(2)}',
                      style: TextStyle(
                        color: Theme.of(context).primaryColor,
                        fontSize: 18.0,
                        fontWeight: FontWeight.bold,
                      ),
                    ),
                  ],
                ),

                const SizedBox(height: 16.0),

                // Finalizar pedido
                SizedBox(
                  height: 48.0,
                  child: ElevatedButton(
                    style: ElevatedButton.styleFrom(
                      foregroundColor: Colors.white,
                      backgroundColor: Theme.of(context).primaryColor,
                    ),
                    onPressed: model.isLoading ? null : buy,
                    child: const Text(
                      'Finalizar Pedido',
                      style: TextStyle(fontSize: 16.0, fontWeight: FontWeight.bold),
                    ),
                  ),
                ),
              ],
            );
          },
        ),
      ),
    );
  }
}
