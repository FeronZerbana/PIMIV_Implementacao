import 'package:flutter/material.dart';
import 'package:scoped_model/scoped_model.dart';
import 'package:apppim/screens/cart_screen.dart';
import 'package:apppim/datas/product_data.dart';
import 'package:apppim/datas/cart_product.dart';
import 'package:apppim/models/cart_model.dart';
import 'package:apppim/models/user_model.dart';
import 'package:apppim/screens/login_screen.dart';
import '../widgets/app_network_image.dart';

class ProductScreen extends StatefulWidget {
  final ProductData product;

  const ProductScreen(this.product, {super.key});

  @override
  State<ProductScreen> createState() => _ProductScreenState();
}

class _ProductScreenState extends State<ProductScreen> {
  String? selectedSize;

  @override
  void initState() {
    super.initState();
    if (widget.product.sizes.isNotEmpty) {
      selectedSize = widget.product.sizes.first;
    } else if (widget.product.modelo.isNotEmpty) {
      selectedSize = widget.product.modelo;
    }
  }

  void _addToCart() {
    final product = widget.product;

    final cartProduct = CartProduct(
      cid: DateTime.now().microsecondsSinceEpoch.toString(),
      category: product.category,
      pid: product.id,
      productData: product,
      variation: selectedSize,
      quantity: 1,
    );

    ScopedModel.of<CartModel>(context).addCartItem(cartProduct);

    ScaffoldMessenger.of(context).showSnackBar(
      SnackBar(
        content: Text(
          '${product.title} adicionado ao carrinho!',
        ),
        duration: const Duration(seconds: 2),
      ),
    );

    Navigator.of(context).push(
      MaterialPageRoute(
        builder: (context) => const CartScreen(),
      ),
    );
  }

  void _openLogin() {
    Navigator.of(context).push(
      MaterialPageRoute(
        builder: (_) => LoginScreen(),
      ),
    );
  }

  @override
  Widget build(BuildContext context) {
    final product = widget.product;
    final primaryColor = Theme.of(context).primaryColor;
    final bool isEsgotado = product.quantidadeEstoque <= 0;
    final hasSizes = product.sizes.isNotEmpty;

    return Scaffold(
      appBar: AppBar(
        title: Text(product.title),
        centerTitle: true,
      ),
      body: ListView(
        padding: const EdgeInsets.all(16),
        children: [
          SizedBox(
            height: 280,
            child: ClipRRect(
              borderRadius: BorderRadius.circular(8),
              child: product.imagens.isNotEmpty
                  ? AppNetworkImage(
                imageUrl: product.imagens[0],
                fit: BoxFit.contain,
              )
                  : const Center(
                child: Icon(
                  Icons.music_note,
                  size: 80,
                  color: Colors.grey,
                ),
              ),
            ),
          ),
          const SizedBox(height: 16),
          Text(
            product.title,
            style: const TextStyle(
              fontSize: 22,
              fontWeight: FontWeight.bold,
            ),
          ),
          const SizedBox(height: 6),
          Row(
            children: [
              Text(
                'R\$ ${product.price.toStringAsFixed(2)}',
                style: TextStyle(
                  fontSize: 22,
                  fontWeight: FontWeight.bold,
                  color: primaryColor,
                ),
              ),
              const Spacer(),
              Container(
                padding: const EdgeInsets.symmetric(horizontal: 10, vertical: 4),
                decoration: BoxDecoration(
                  color: isEsgotado ? Colors.red.shade50 : Colors.green.shade50,
                  borderRadius: BorderRadius.circular(6),
                  border: Border.all(
                    color: isEsgotado ? Colors.red : Colors.green,
                  ),
                ),
                child: Text(
                  isEsgotado ? 'Esgotado' : 'Estoque: ${product.quantidadeEstoque}',
                  style: TextStyle(
                    fontSize: 12,
                    fontWeight: FontWeight.bold,
                    color: isEsgotado ? Colors.red.shade900 : Colors.green.shade900,
                  ),
                ),
              ),
            ],
          ),
          if (product.modelo.isNotEmpty) ...[
            const SizedBox(height: 12),
            Text(
              'Modelo: ${product.modelo}',
              style: TextStyle(
                fontSize: 14,
                color: Colors.grey.shade700,
                fontWeight: FontWeight.w500,
              ),
            ),
          ],
          if (hasSizes) ...[
            const SizedBox(height: 18),
            const Text(
              'Tamanho / Especificacao:',
              style: TextStyle(
                fontSize: 15,
                fontWeight: FontWeight.w600,
              ),
            ),
            const SizedBox(height: 8),
            Wrap(
              spacing: 8,
              runSpacing: 8,
              children: product.sizes.map((size) {
                final isSelected = selectedSize == size;

                return OutlinedButton(
                  onPressed: () {
                    setState(() {
                      selectedSize = size;
                    });
                  },
                  style: OutlinedButton.styleFrom(
                    side: BorderSide(
                      color: isSelected ? primaryColor : Colors.grey,
                      width: isSelected ? 2 : 1,
                    ),
                  ),
                  child: Text(size),
                );
              }).toList(),
            ),
          ],
          const SizedBox(height: 24),
          ScopedModelDescendant<UserModel>(
            builder: (context, child, userModel) {
              final isLoggedIn = userModel.isLoggedIn();

              if (isEsgotado) {
                return SizedBox(
                  height: 48,
                  child: ElevatedButton(
                    style: ElevatedButton.styleFrom(
                      backgroundColor: Colors.grey.shade400,
                      foregroundColor: Colors.white,
                    ),
                    onPressed: null,
                    child: const Text('Produto Esgotado'),
                  ),
                );
              }

              return SizedBox(
                height: 48,
                child: ElevatedButton(
                  style: ElevatedButton.styleFrom(
                    backgroundColor: primaryColor,
                    foregroundColor: Colors.white,
                  ),
                  onPressed: !isLoggedIn
                      ? _openLogin
                      : _addToCart,
                  child: Text(
                    isLoggedIn
                        ? 'Adicionar ao Carrinho'
                        : 'Faca login para comprar',
                    style: const TextStyle(fontSize: 16, fontWeight: FontWeight.bold),
                  ),
                ),
              );
            },
          ),
          const SizedBox(height: 24),
          const Text(
            'Descricao do Instrumento',
            style: TextStyle(
              fontSize: 17,
              fontWeight: FontWeight.w600,
            ),
          ),
          const SizedBox(height: 8),
          Text(
            product.description,
            style: const TextStyle(fontSize: 15, height: 1.4),
          ),
        ],
      ),
    );
  }
}
