import 'package:flutter/material.dart';
import '../datas/product_data.dart';
import '../services/product_service.dart';
import '../tiles/product_tile.dart';
import '../widgets/server_settings_dialog.dart';

class HomeTab extends StatefulWidget {
  const HomeTab({super.key});

  @override
  State<HomeTab> createState() => _HomeTabState();
}

class _HomeTabState extends State<HomeTab> {
  late Future<List<ProductData>> _productsFuture;

  @override
  void initState() {
    super.initState();
    _loadProducts();
  }

  void _loadProducts() {
    setState(() {
      _productsFuture = ProductService.getProducts();
    });
  }

  @override
  Widget build(BuildContext context) {
    Widget buildBodyBack() => Container(
      decoration: const BoxDecoration(
        gradient: LinearGradient(
          colors: [
            Color.fromARGB(255, 75, 0, 130),
            Color.fromARGB(255, 255, 255, 255),
          ],
          begin: Alignment.topLeft,
          end: Alignment.bottomRight,
        ),
      ),
    );

    return Stack(
      children: <Widget>[
        buildBodyBack(),
        RefreshIndicator(
          color: Theme.of(context).primaryColor,
          onRefresh: () async {
            _loadProducts();
            await _productsFuture;
          },
          child: CustomScrollView(
            physics: const AlwaysScrollableScrollPhysics(),
            slivers: [
              SliverAppBar(
                floating: true,
                snap: true,
                backgroundColor: Colors.transparent,
                elevation: 0,
                iconTheme: const IconThemeData(
                  color: Colors.white,
                ),
                actions: [
                  IconButton(
                    icon: const Icon(Icons.refresh, color: Colors.white),
                    tooltip: 'Atualizar Produtos',
                    onPressed: _loadProducts,
                  ),
                  IconButton(
                    icon: const Icon(Icons.dns, color: Colors.white),
                    tooltip: 'Configurar Servidor / API',
                    onPressed: () async {
                      await ServerSettingsDialog.show(context);
                      _loadProducts();
                    },
                  ),
                ],
                flexibleSpace: const FlexibleSpaceBar(
                  title: Text(
                    "Novidades & Destaques",
                    style: TextStyle(
                      color: Colors.white,
                      fontWeight: FontWeight.bold,
                    ),
                  ),
                  centerTitle: true,
                ),
              ),
              SliverToBoxAdapter(
                child: FutureBuilder<List<ProductData>>(
                  future: _productsFuture,
                  builder: (context, snapshot) {
                    if (snapshot.connectionState == ConnectionState.waiting) {
                      return const Padding(
                        padding: EdgeInsets.symmetric(vertical: 60),
                        child: Center(
                          child: CircularProgressIndicator(color: Colors.white),
                        ),
                      );
                    }

                    if (snapshot.hasError) {
                      return Padding(
                        padding: const EdgeInsets.all(24),
                        child: Column(
                          children: [
                            const Icon(Icons.cloud_off, size: 50, color: Colors.white),
                            const SizedBox(height: 12),
                            const Text(
                              'Nao foi possivel conectar ao servidor da loja.',
                              style: TextStyle(color: Colors.white, fontWeight: FontWeight.bold),
                              textAlign: TextAlign.center,
                            ),
                            const SizedBox(height: 12),
                            ElevatedButton.icon(
                              icon: const Icon(Icons.refresh),
                              label: const Text('Tentar Novamente'),
                              onPressed: _loadProducts,
                            ),
                          ],
                        ),
                      );
                    }

                    final produtos = snapshot.data ?? [];
                    if (produtos.isEmpty) {
                      return Padding(
                        padding: const EdgeInsets.symmetric(horizontal: 24, vertical: 48),
                        child: Center(
                          child: Column(
                            mainAxisSize: MainAxisSize.min,
                            children: [
                              const Icon(Icons.storefront_outlined, size: 64, color: Colors.white70),
                              const SizedBox(height: 16),
                              const Text(
                                'Nenhum instrumento musical disponivel no momento.',
                                style: TextStyle(color: Colors.white, fontSize: 16, fontWeight: FontWeight.w500),
                                textAlign: TextAlign.center,
                              ),
                              const SizedBox(height: 16),
                              ElevatedButton.icon(
                                icon: const Icon(Icons.refresh),
                                label: const Text('Atualizar Catalogo'),
                                onPressed: _loadProducts,
                              ),
                            ],
                          ),
                        ),
                      );
                    }

                    return GridView.builder(
                      padding: const EdgeInsets.all(8),
                      shrinkWrap: true,
                      physics: const NeverScrollableScrollPhysics(),
                      gridDelegate: const SliverGridDelegateWithFixedCrossAxisCount(
                        crossAxisCount: 2,
                        mainAxisSpacing: 8,
                        crossAxisSpacing: 8,
                        childAspectRatio: 0.72,
                      ),
                      itemCount: produtos.length,
                      itemBuilder: (context, index) {
                        return ProductTile('grid', produtos[index]);
                      },
                    );
                  },
                ),
              ),
            ],
          ),
        ),
      ],
    );
  }
}
