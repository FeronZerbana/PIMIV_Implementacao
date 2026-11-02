import 'package:flutter/material.dart';
import '../datas/product_data.dart';
import '../screens/product_screen.dart';
import '../widgets/app_network_image.dart';

class ProductTile extends StatelessWidget {
  final String type;
  final ProductData product;

  const ProductTile(this.type, this.product, {super.key});

  @override
  Widget build(BuildContext context) {
    final primaryColor = Theme.of(context).primaryColor;
    final imgUrl = product.imagens.isNotEmpty ? product.imagens[0] : '';
    final bool isEsgotado = product.quantidadeEstoque <= 0;

    return Card(
      clipBehavior: Clip.antiAlias,
      elevation: 2,
      child: InkWell(
        onTap: () {
          Navigator.of(context).push(
            MaterialPageRoute(
              builder: (context) => ProductScreen(product),
            ),
          );
        },
        child: type == 'grid'
            ? Column(
          crossAxisAlignment: CrossAxisAlignment.stretch,
          children: [
            Expanded(
              child: Stack(
                fit: StackFit.expand,
                children: [
                  AppNetworkImage(
                    imageUrl: imgUrl,
                    fit: BoxFit.cover,
                  ),
                  if (isEsgotado)
                    Positioned(
                      top: 8,
                      right: 8,
                      child: Container(
                        padding: const EdgeInsets.symmetric(horizontal: 6, vertical: 2),
                        decoration: BoxDecoration(
                          color: Colors.red.shade700,
                          borderRadius: BorderRadius.circular(4),
                        ),
                        child: const Text(
                          'Esgotado',
                          style: TextStyle(color: Colors.white, fontSize: 10, fontWeight: FontWeight.bold),
                        ),
                      ),
                    ),
                ],
              ),
            ),
            Padding(
              padding: const EdgeInsets.all(8.0),
              child: Column(
                crossAxisAlignment: CrossAxisAlignment.start,
                children: [
                  Text(
                    product.title,
                    maxLines: 2,
                    overflow: TextOverflow.ellipsis,
                    style: const TextStyle(
                      fontWeight: FontWeight.w600,
                      fontSize: 14,
                    ),
                  ),
                  const SizedBox(height: 4),
                  Text(
                    'R\$ ${product.price.toStringAsFixed(2)}',
                    style: TextStyle(
                      color: primaryColor,
                      fontSize: 15,
                      fontWeight: FontWeight.bold,
                    ),
                  ),
                  if (!isEsgotado)
                    Text(
                      'Estoque: ${product.quantidadeEstoque}',
                      style: TextStyle(fontSize: 11, color: Colors.grey.shade600),
                    ),
                ],
              ),
            ),
          ],
        )
            : Row(
          children: [
            SizedBox(
              width: 110,
              height: 110,
              child: Stack(
                fit: StackFit.expand,
                children: [
                  AppNetworkImage(
                    imageUrl: imgUrl,
                    fit: BoxFit.cover,
                  ),
                  if (isEsgotado)
                    Positioned(
                      top: 4,
                      left: 4,
                      child: Container(
                        padding: const EdgeInsets.symmetric(horizontal: 4, vertical: 2),
                        decoration: BoxDecoration(
                          color: Colors.red.shade700,
                          borderRadius: BorderRadius.circular(4),
                        ),
                        child: const Text(
                          'Esgotado',
                          style: TextStyle(color: Colors.white, fontSize: 9, fontWeight: FontWeight.bold),
                        ),
                      ),
                    ),
                ],
              ),
            ),
            Expanded(
              child: Padding(
                padding: const EdgeInsets.all(12.0),
                child: Column(
                  crossAxisAlignment: CrossAxisAlignment.start,
                  children: [
                    Text(
                      product.title,
                      style: const TextStyle(
                        fontWeight: FontWeight.w600,
                        fontSize: 15,
                      ),
                    ),
                    const SizedBox(height: 4),
                    Text(
                      'R\$ ${product.price.toStringAsFixed(2)}',
                      style: TextStyle(
                        color: primaryColor,
                        fontSize: 16,
                        fontWeight: FontWeight.bold,
                      ),
                    ),
                    if (!isEsgotado)
                      Text(
                        'Estoque disponivel: ${product.quantidadeEstoque}',
                        style: TextStyle(fontSize: 11, color: Colors.grey.shade600),
                      ),
                  ],
                ),
              ),
            ),
          ],
        ),
      ),
    );
  }
}
