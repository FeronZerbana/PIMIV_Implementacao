import 'package:flutter/material.dart';
import 'package:apppim/models/user_model.dart';
import 'package:apppim/screens/login_screen.dart';
import 'package:apppim/services/order_service.dart';
import 'package:apppim/tiles/order_tile.dart';

class OrdersTab extends StatefulWidget {
  const OrdersTab({super.key});

  @override
  State<OrdersTab> createState() => _OrdersTabState();
}

class _OrdersTabState extends State<OrdersTab> {
  Future<List<Map<String, dynamic>>>? _ordersFuture;

  @override
  void initState() {
    super.initState();
    _loadOrders();
  }

  void _loadOrders() {
    final userModel = UserModel.of(context);
    final int? codCliente = userModel.idCliente;

    if (codCliente != null && codCliente > 0) {
      setState(() {
        _ordersFuture = OrderService.getCustomerOrders(codCliente);
      });
    }
  }

  @override
  Widget build(BuildContext context) {
    final userModel = UserModel.of(context);

    if (!userModel.isLoggedIn()) {
      return Center(
        child: Padding(
          padding: const EdgeInsets.all(24.0),
          child: Column(
            mainAxisSize: MainAxisSize.min,
            crossAxisAlignment: CrossAxisAlignment.stretch,
            children: [
              Icon(
                Icons.receipt_long_outlined,
                size: 80.0,
                color: Theme.of(context).primaryColor,
              ),
              const SizedBox(height: 16.0),
              const Text(
                'Faca login para acessar seus pedidos!',
                style: TextStyle(
                  fontSize: 18.0,
                  fontWeight: FontWeight.bold,
                ),
                textAlign: TextAlign.center,
              ),
              const SizedBox(height: 16.0),
              ElevatedButton(
                style: ElevatedButton.styleFrom(
                  foregroundColor: Colors.white,
                  backgroundColor: Theme.of(context).primaryColor,
                ),
                onPressed: () async {
                  await Navigator.of(context).push(
                    MaterialPageRoute(
                      builder: (_) => LoginScreen(),
                    ),
                  );
                  _loadOrders();
                },
                child: const Text(
                  'Entrar',
                  style: TextStyle(fontSize: 16.0),
                ),
              ),
            ],
          ),
        ),
      );
    }

    final int? codCliente = userModel.idCliente;
    if (_ordersFuture == null && codCliente != null) {
      _loadOrders();
    }

    return Scaffold(
      appBar: AppBar(
        title: const Text('Meus Pedidos'),
        centerTitle: true,
        actions: [
          IconButton(
            icon: const Icon(Icons.refresh),
            tooltip: 'Atualizar Pedidos',
            onPressed: _loadOrders,
          ),
        ],
      ),
      body: FutureBuilder<List<Map<String, dynamic>>>(
        future: _ordersFuture,
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
                    const Text('Erro ao carregar historico de compras.'),
                    const SizedBox(height: 12),
                    ElevatedButton(
                      onPressed: _loadOrders,
                      child: const Text('Recarregar'),
                    ),
                  ],
                ),
              ),
            );
          }

          final orders = snapshot.data ?? [];

          if (orders.isEmpty) {
            return Center(
              child: Padding(
                padding: const EdgeInsets.all(24.0),
                child: Column(
                  mainAxisSize: MainAxisSize.min,
                  children: [
                    Icon(
                      Icons.receipt_long,
                      size: 70.0,
                      color: Theme.of(context).primaryColor,
                    ),
                    const SizedBox(height: 16.0),
                    const Text(
                      'Voce ainda nao possui pedidos!',
                      style: TextStyle(
                        fontSize: 18.0,
                        fontWeight: FontWeight.bold,
                      ),
                      textAlign: TextAlign.center,
                    ),
                    const SizedBox(height: 8.0),
                    const Text(
                      'Assim que concluir uma compra, ela aparecera aqui.',
                      textAlign: TextAlign.center,
                      style: TextStyle(fontSize: 14.0, color: Colors.grey),
                    ),
                  ],
                ),
              ),
            );
          }

          return ListView.builder(
            padding: const EdgeInsets.symmetric(vertical: 8.0),
            itemCount: orders.length,
            itemBuilder: (context, index) {
              final v = orders[index];
              final String idStr = (v['idVenda'] ?? '').toString();
              DateTime data = DateTime.now();
              if (v['dataVenda'] != null) {
                data = DateTime.tryParse(v['dataVenda'].toString()) ?? DateTime.now();
              }
              final double total = double.tryParse(v['totalVenda']?.toString() ?? '0') ?? 0.0;
              final double subtotal = double.tryParse(v['subtotal']?.toString() ?? '0') ?? 0.0;
              final double valorFrete = double.tryParse(v['valorFrete']?.toString() ?? '0') ?? 0.0;
              final double valorDesconto = double.tryParse(v['valorDesconto']?.toString() ?? '0') ?? 0.0;
              final String? codigoCupom = v['codigoCupom']?.toString();
              final List itens = (v['itens'] is List) ? v['itens'] as List : [];
              final int itemCount = itens.isNotEmpty
                  ? itens.fold<int>(0, (acc, item) => acc + (int.tryParse(item['quantidade']?.toString() ?? '1') ?? 1))
                  : 1;

              return OrderTile(
                idStr,
                orderDate: data,
                itemCount: itemCount,
                total: total,
                subtotal: subtotal > 0 ? subtotal : null,
                valorFrete: valorFrete,
                valorDesconto: valorDesconto,
                codigoCupom: codigoCupom,
                itens: itens,
                currentStep: 2, // Em transporte / confirmado
              );
            },
          );
        },
      ),
    );
  }
}
