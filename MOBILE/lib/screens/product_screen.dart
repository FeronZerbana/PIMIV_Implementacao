
import 'package:flutter/material.dart';
import 'package:scoped_model/scoped_model.dart';
import 'package:apppim/screens/cart_screen.dart';
import 'package:apppim/datas/product_data.dart';
import 'package:apppim/datas/cart_product.dart';
import 'package:apppim/models/cart_model.dart';
import 'package:apppim/models/user_model.dart';
import 'package:apppim/screens/login_screen.dart';

class ProductScreen extends StatefulWidget {
  final ProductData product;

  const ProductScreen(this.product, {super.key});

  @override
  State<ProductScreen> createState() => _ProductScreenState();
}

class _ProductScreenState extends State<ProductScreen> {
  String? selectedSize;

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
          '${product.title} foi adicionado ao carrinho!',
        ),
        duration: const Duration(seconds: 2),
      ),
    );

    Navigator.of(context).push(
      MaterialPageRoute(
        builder: (context) => CartScreen(),
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
    final requiresVariation = product.sizes.isNotEmpty;

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
            child: product.imagens.isEmpty
                ? const Center(
              child: Icon(
                Icons.image_not_supported,
                size: 60,
              ),
            )
                : PageView(
              children: product.imagens.map((imagem) {
                return Image.asset(
                  imagem,
                  fit: BoxFit.contain,
                  errorBuilder: (context, error, stackTrace) {
                    return const Center(
                      child: Icon(
                        Icons.image_not_supported,
                        size: 60,
                      ),
                    );
                  },
                );
              }).toList(),
            ),
          ),

          const SizedBox(height: 16),

          Text(
            product.title,
            style: const TextStyle(
              fontSize: 22,
              fontWeight: FontWeight.w500,
            ),
          ),

          const SizedBox(height: 8),

          Text(
            'R\$ ${product.price.toStringAsFixed(2)}',
            style: TextStyle(
              fontSize: 22,
              fontWeight: FontWeight.bold,
              color: primaryColor,
            ),
          ),

          if (requiresVariation) ...[
            const SizedBox(height: 20),

            const Text(
              'Tamanho / Modelo',
              style: TextStyle(
                fontSize: 16,
                fontWeight: FontWeight.w500,
              ),
            ),

            const SizedBox(height: 10),

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
          const SizedBox(height: 20),
          ScopedModelDescendant<UserModel>(
            builder: (context, child, userModel) {
              final isLoggedIn = userModel.isLoggedIn();

              return SizedBox(
                height: 48,
                child: ElevatedButton(
                  onPressed: !isLoggedIn
                      ? _openLogin
                      : (requiresVariation && selectedSize == null)
                      ? null
                      : _addToCart,
                  child: Text(
                    isLoggedIn
                        ? 'Adicionar ao carrinho'
                        : 'Faça login para adicionar ao carrinho',
                  ),
                ),
              );
            },
          ),

          const SizedBox(height: 24),

          const Text(
            'Descrição',
            style: TextStyle(
              fontSize: 18,
              fontWeight: FontWeight.w500,
            ),
          ),

          const SizedBox(height: 8),

          Text(
            product.description,
            style: const TextStyle(fontSize: 16),
          ),
        ],
      ),
    );
  }
}
