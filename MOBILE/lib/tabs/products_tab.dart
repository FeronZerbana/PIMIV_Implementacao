import 'package:apppim/tiles/category_tile.dart';
import 'package:flutter/material.dart';

class ProductsTab extends StatelessWidget {
  ProductsTab({super.key});

  final List<Map<String, String>> categories = [
    {
      'title': 'Guitarras',
      'icon': 'assets/imagens/guitarramenu.png',
    },
    {
      'title': 'Violoes',
      'icon': 'assets/imagens/instrumentos.jpg',
    },
    {
      'title': 'Baterias',
      'icon': 'assets/imagens/bateriamenu.png',
    },
    {
      'title': 'Teclados',
      'icon': 'assets/imagens/amplificador.jpg',
    },
    {
      'title': 'Acessorios',
      'icon': 'assets/imagens/baqtmenu.png',
    },
    {
      'title': 'Outros',
      'icon': 'assets/imagens/violino.jpg',
    },
  ];

  @override
  Widget build(BuildContext context) {
    final dividedTiles = ListTile.divideTiles(
      context: context,
      color: Colors.grey[300],
      tiles: categories.map((category) {
        return CategoryTile(
          title: category['title']!,
          imagePath: category['icon']!,
        );
      }).toList(),
    ).toList();

    return ListView(
      children: dividedTiles,
    );
  }
}
