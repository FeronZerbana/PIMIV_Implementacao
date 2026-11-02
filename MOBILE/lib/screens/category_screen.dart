import 'package:flutter/material.dart';
import '../datas/product_data.dart';
import '../services/product_service.dart';
import '../tiles/product_tile.dart';

class CategoryScreen extends StatefulWidget {
  final String title;

  const CategoryScreen(this.title, {super.key});

  @override
  State<CategoryScreen> createState() => _CategoryScreenState();
}

class _CategoryScreenState extends State<CategoryScreen> {
  late Future<List<ProductData>> _categoryFuture;

  @override
  void initState() {
    super.initState();
    _loadCategory();
  }

  void _loadCategory() {
    setState(() {
      _categoryFuture = ProductService.getProductsByCategory(widget.title);
    });
  }

  @override
  Widget build(BuildContext context) {
    return DefaultTabController(
      length: 2,
      child: Scaffold(
        appBar: AppBar(
          title: Text(widget.title),
          centerTitle: true,
          actions: [
            IconButton(
              icon: const Icon(Icons.refresh),
              tooltip: 'Atualizar',
              onPressed: _loadCategory,
            ),
          ],
          bottom: const TabBar(
            indicatorColor: Colors.white,
            tabs: [
              Tab(icon: Icon(Icons.grid_on)),
              Tab(icon: Icon(Icons.list)),
            ],
          ),
        ),
        body: FutureBuilder<List<ProductData>>(
          future: _categoryFuture,
          builder: (context, snapshot) {
            if (snapshot.connectionState == ConnectionState.waiting) {
              return const Center(
                child: CircularProgressIndicator(),
              );
            }

            if (snapshot.hasError) {
              return Center(
                child: Padding(
                  padding: const EdgeInsets.all(16.0),
                  child: Column(
                    mainAxisSize: MainAxisSize.min,
                    children: [
                      const Icon(Icons.error_outline, size: 50, color: Colors.red),
                      const SizedBox(height: 12),
                      const Text(
                        'Erro ao carregar produtos desta categoria.',
                        style: TextStyle(fontWeight: FontWeight.bold),
                      ),
                      const SizedBox(height: 12),
                      ElevatedButton(
                        onPressed: _loadCategory,
                        child: const Text('Recarregar'),
                      ),
                    ],
                  ),
                ),
              );
            }

            final categoryProducts = snapshot.data ?? [];

            if (categoryProducts.isEmpty) {
              return Center(
                child: Padding(
                  padding: const EdgeInsets.all(24.0),
                  child: Column(
                    mainAxisSize: MainAxisSize.min,
                    children: [
                      Icon(Icons.inventory_2_outlined, size: 60, color: Colors.grey.shade400),
                      const SizedBox(height: 16),
                      Text(
                        'Nenhum instrumento cadastrado na categoria ${widget.title}.',
                        textAlign: TextAlign.center,
                        style: const TextStyle(fontSize: 16, color: Colors.grey),
                      ),
                    ],
                  ),
                ),
              );
            }

            return TabBarView(
              physics: const NeverScrollableScrollPhysics(),
              children: [
                GridView.builder(
                  padding: const EdgeInsets.all(6),
                  gridDelegate: const SliverGridDelegateWithFixedCrossAxisCount(
                    crossAxisCount: 2,
                    mainAxisSpacing: 6,
                    crossAxisSpacing: 6,
                    childAspectRatio: 0.68,
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
                  padding: const EdgeInsets.all(6),
                  itemCount: categoryProducts.length,
                  itemBuilder: (context, index) {
                    return ProductTile(
                      'list',
                      categoryProducts[index],
                    );
                  },
                ),
              ],
            );
          },
        ),
      ),
    );
  }
}
