
import 'package:flutter/material.dart';

import 'package:apppim/tabs/home_tab.dart';
import 'package:apppim/tabs/products_tab.dart';
import 'package:apppim/tabs/orders_tab.dart';

import 'package:apppim/widgets/cart_button.dart';
import 'package:apppim/widgets/custom_drawer.dart';

class HomeScreen extends StatefulWidget {
  const HomeScreen({super.key});

  @override
  State<HomeScreen> createState() => _HomeScreenState();
}

class _HomeScreenState extends State<HomeScreen> {
  final PageController _pageController = PageController();

  @override
  void dispose() {
    _pageController.dispose();
    super.dispose();
  }

  @override
  Widget build(BuildContext context) {
    return PageView(
      controller: _pageController,
      physics: const NeverScrollableScrollPhysics(),
      children: [
        // Página inicial.
        Scaffold(
          body: HomeTab(),
          drawer: CustomDrawer(_pageController),
          floatingActionButton: CartButton(),
        ),

        // Página de produtos.
        Scaffold(
          appBar: AppBar(
            title: const Text('Produtos'),
            centerTitle: true,
          ),
          drawer: CustomDrawer(_pageController),
          body: ProductsTab(),
          floatingActionButton: CartButton(),
        ),

        // Meus pedidos.
        Scaffold(
          appBar: AppBar(
            title: const Text('Meus Pedidos'),
            centerTitle: true,
          ),
          body: OrdersTab(),
          drawer: CustomDrawer(_pageController),
        ),
      ],
    );
  }
}
