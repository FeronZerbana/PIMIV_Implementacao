
import 'package:flutter/material.dart';
import 'package:scoped_model/scoped_model.dart';

import 'package:apppim/datas/cart_product.dart';
import 'package:apppim/models/cart_model.dart';

class CartTile extends StatelessWidget {
  final CartProduct cartProduct;

  const CartTile(this.cartProduct, {super.key});

  @override
  Widget build(BuildContext context) {
    final product = cartProduct.productData;

    return ScopedModelDescendant<CartModel>(
      builder: (context, child, cartModel) {
        return Card(
          margin: const EdgeInsets.symmetric(
            horizontal: 8.0,
            vertical: 4.0,
          ),
          child: Padding(
            padding: const EdgeInsets.all(8.0),
            child: Row(
              children: [
                // Imagem do produto.
                SizedBox(
                  width: 100,
                  height: 120,
                  child: product.imagens.isNotEmpty
                      ? Image.asset(
                    product.imagens[0],
                    fit: BoxFit.contain,
                    errorBuilder: (context, error, stackTrace) {
                      return const Icon(
                        Icons.image_not_supported,
                        size: 50,
                      );
                    },
                  )
                      : const Icon(
                    Icons.image_not_supported,
                    size: 50,
                  ),
                ),

                // Informações do produto.
                Expanded(
                  child: Padding(
                    padding: const EdgeInsets.all(8.0),
                    child: Column(
                      crossAxisAlignment: CrossAxisAlignment.start,
                      children: [
                        Text(
                          product.title,
                          style: const TextStyle(
                            fontWeight: FontWeight.w500,
                            fontSize: 17.0,
                          ),
                        ),

                        Text(
                          "Tamanho: ${cartProduct.variation ?? 'Único'}",
                          style: const TextStyle(
                            fontWeight: FontWeight.w300,
                          ),
                        ),

                        Text(
                          "R\$ ${product.price.toStringAsFixed(2)}",
                          style: TextStyle(
                            color: Theme.of(context).primaryColor,
                            fontSize: 16.0,
                            fontWeight: FontWeight.bold,
                          ),
                        ),

                        // Quantidade e botão remover.
                        Row(
                          children: [
                            // Diminuir quantidade.
                            IconButton(
                              icon: const Icon(Icons.remove),
                              color: Theme.of(context).primaryColor,
                              onPressed: cartProduct.quantity > 1
                                  ? () {
                                cartModel.decProduct(cartProduct);
                              }
                                  : null,
                            ),

                            // Quantidade atual.
                            Text(
                              cartProduct.quantity.toString(),
                              style: const TextStyle(
                                fontSize: 16.0,
                              ),
                            ),

                            // Aumentar quantidade.
                            IconButton(
                              icon: const Icon(Icons.add),
                              color: Theme.of(context).primaryColor,
                              onPressed: () {
                                cartModel.incProduct(cartProduct);
                              },
                            ),

                            // Remover produto.
                            TextButton(
                              onPressed: () {
                                cartModel.removeCartItem(cartProduct);
                              },
                              child: const Text(
                                'Remover',
                                style: TextStyle(
                                  color: Colors.grey,
                                ),
                              ),
                            ),
                          ],
                        ),

                        // Subtotal deste produto.
                        Text(
                          'Subtotal: R\$ ${cartProduct.totalPrice.toStringAsFixed(2)}',
                          style: const TextStyle(
                            fontWeight: FontWeight.w500,
                            fontSize: 14.0,
                          ),
                        ),
                      ],
                    ),
                  ),
                ),
              ],
            ),
          ),
        );
      },
    );
  }
}
