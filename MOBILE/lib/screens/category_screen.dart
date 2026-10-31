
import 'package:flutter/material.dart';

import '../datas/product_data.dart';
import '../data/store_data.dart';
import '../tiles/product_tile.dart';

class CategoryScreen extends StatelessWidget {
  final String title;

  const CategoryScreen(this.title, {super.key});

  @override
  Widget build(BuildContext context) {
    final categoryProducts = products.where((product) {
      return product.category.toLowerCase() == title.toLowerCase();
    }).toList();

    return DefaultTabController(
      length: 2,
      child: Scaffold(
        appBar: AppBar(
          title: Text(title),
          centerTitle: true,
          bottom: const TabBar(
            indicatorColor: Colors.white,
            tabs: [
              Tab(icon: Icon(Icons.grid_on)),
              Tab(icon: Icon(Icons.list)),
            ],
          ),
        ),
        body: categoryProducts.isEmpty
            ? const Center(
          child: Text('Nenhum produto cadastrado nesta categoria.'),
        )
            : TabBarView(
          physics: const NeverScrollableScrollPhysics(),
          children: [
            GridView.builder(
              padding: const EdgeInsets.all(4),
              gridDelegate:
              const SliverGridDelegateWithFixedCrossAxisCount(
                crossAxisCount: 2,
                mainAxisSpacing: 4,
                crossAxisSpacing: 4,
                childAspectRatio: 0.65,
              ),
              itemCount: categoryProducts.length,
              itemBuilder: (context, index) {
                return ProductTile(
                  'grid',
                  categoryProducts[index],
                );
              },
            ),
            ListView.builder(
              padding: const EdgeInsets.all(4),
              itemCount: categoryProducts.length,
              itemBuilder: (context, index) {
                return ProductTile(
                  'list',
                  categoryProducts[index],
                );
              },
            ),
          ],
        ),
      ),
    );
  }
}

