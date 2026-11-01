import 'package:flutter/material.dart';
import 'package:scoped_model/scoped_model.dart';
import '../datas/cart_product.dart';
import '../models/cart_model.dart';
import '../widgets/app_network_image.dart';

class CartTile extends StatelessWidget {
  final CartProduct cartProduct;

  const CartTile(this.cartProduct, {super.key});

  @override
  Widget build(BuildContext context) {
    final product = cartProduct.productData;
    final imgUrl = product.imagens.isNotEmpty ? product.imagens[0] : '';
    final primaryColor = Theme.of(context).primaryColor;

    return ScopedModelDescendant<CartModel>(
      builder: (context, child, cartModel) {
        final bool canInc = product.quantidadeEstoque <= 0 || cartProduct.quantity < product.quantidadeEstoque;

        return Card(
          margin: const EdgeInsets.symmetric(
            horizontal: 8.0,
            vertical: 4.0,
          ),
          child: Padding(
            padding: const EdgeInsets.all(8.0),
            child: Row(
              children: [
                SizedBox(
                  width: 90,
                  height: 100,
                  child: ClipRRect(
                    borderRadius: BorderRadius.circular(4),
                    child: AppNetworkImage(
                      imageUrl: imgUrl,
                      fit: BoxFit.cover,
                    ),
                  ),
                ),
                Expanded(
                  child: Padding(
                    padding: const EdgeInsets.symmetric(horizontal: 10.0),
                    child: Column(
                      crossAxisAlignment: CrossAxisAlignment.start,
                      children: [
                        Text(
                          product.title,
                          maxLines: 2,
                          overflow: TextOverflow.ellipsis,
                          style: const TextStyle(
                            fontWeight: FontWeight.w600,
                            fontSize: 15.0,
                          ),
                        ),
                        if (cartProduct.variation != null && cartProduct.variation!.isNotEmpty)
                          Text(
                            "Modelo/Variacao: ${cartProduct.variation}",
                            style: const TextStyle(
                              fontWeight: FontWeight.w300,
                              fontSize: 12.0,
                            ),
                          ),
                        const SizedBox(height: 4),
                        Text(
                          "R\$ ${product.price.toStringAsFixed(2)}",
                          style: TextStyle(
                            color: primaryColor,
                            fontSize: 15.0,
                            fontWeight: FontWeight.bold,
                          ),
                        ),
                        const SizedBox(height: 6),
                        Row(
                          children: [
                            IconButton(
                              icon: const Icon(Icons.remove, size: 18),
                              color: primaryColor,
                              padding: EdgeInsets.zero,
                              constraints: const BoxConstraints(),
                              onPressed: () {
                                cartModel.decProduct(cartProduct);
                              },
                            ),
                            Padding(
                              padding: const EdgeInsets.symmetric(horizontal: 10.0),
                              child: Text(
                                cartProduct.quantity.toString(),
                                style: const TextStyle(
                                  fontSize: 15.0,
                                  fontWeight: FontWeight.bold,
                                ),
                              ),
                            ),
                            IconButton(
                              icon: const Icon(Icons.add, size: 18),
                              color: canInc ? primaryColor : Colors.grey,
                              padding: EdgeInsets.zero,
                              constraints: const BoxConstraints(),
                              onPressed: canInc
                                  ? () {
                                cartModel.incProduct(cartProduct);
                              }
                                  : null,
                            ),
                            const Spacer(),
                            TextButton(
                              style: TextButton.styleFrom(
                                padding: EdgeInsets.zero,
                                foregroundColor: Colors.grey.shade700,
                              ),
                              onPressed: () {
                                cartModel.removeCartItem(cartProduct);
                              },
                              child: const Text(
                                "Remover",
                                style: TextStyle(fontSize: 12),
                              ),
                            ),
                          ],
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
