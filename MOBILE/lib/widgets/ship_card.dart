import 'package:flutter/material.dart';
import '../models/cart_model.dart';

class ShipCard extends StatefulWidget {
  const ShipCard({super.key});

  @override
  State<ShipCard> createState() => _ShipCardState();
}

class _ShipCardState extends State<ShipCard> {
  final TextEditingController _cepController = TextEditingController();
  bool _isLoading = false;
  String? _calculatedCep;

  @override
  void dispose() {
    _cepController.dispose();
    super.dispose();
  }

  void _calculateShipping() {
    final cep = _cepController.text.trim();

    if (cep.isEmpty) {
      _showMessage('Digite seu CEP.');
      return;
    }

    if (!RegExp(r'^\d{5}-?\d{3}$').hasMatch(cep)) {
      _showMessage('Digite um CEP valido (ex: 01001-000).');
      return;
    }

    setState(() {
      _isLoading = true;
    });

    // Taxa fixa padrao de R$ 9,99 conforme alinhado entre plataformas
    double calculated = 9.99;

    // Regra de Frete Gratis para compras acima de R$ 800
    final cartModel = CartModel.of(context);
    if (cartModel.getProductsPrice() >= 800) {
      calculated = 0.0;
    }

    cartModel.setShippingPrice(calculated);

    setState(() {
      _isLoading = false;
      _calculatedCep = cep;
    });

    _showMessage(
      calculated == 0.0
          ? 'Frete Gratis aplicado para compras acima de R\$ 800!'
          : 'Frete fixo calculado: R\$ ${calculated.toStringAsFixed(2)}',
    );
  }

  void _showMessage(String message) {
    ScaffoldMessenger.of(context).showSnackBar(
      SnackBar(
        content: Text(message),
        duration: const Duration(seconds: 2),
      ),
    );
  }

  @override
  Widget build(BuildContext context) {
    final cartModel = CartModel.of(context);

    return Card(
      margin: const EdgeInsets.symmetric(
        horizontal: 8.0,
        vertical: 4.0,
      ),
      child: ExpansionTile(
        title: Text(
          _calculatedCep != null
              ? (cartModel.getShipPrice() == 0.0
                  ? 'Frete: Gratis ($_calculatedCep)'
                  : 'Frete: R\$ ${cartModel.getShipPrice().toStringAsFixed(2)} ($_calculatedCep)')
              : 'Calcular Frete e Entrega',
          textAlign: TextAlign.start,
          style: TextStyle(
            fontWeight: FontWeight.w500,
            color: _calculatedCep != null ? Theme.of(context).primaryColor : Colors.grey.shade700,
          ),
        ),
        leading: Icon(
          Icons.location_on,
          color: Theme.of(context).primaryColor,
        ),
        children: [
          Padding(
            padding: const EdgeInsets.all(8.0),
            child: Form(
              child: Column(
                children: [
                  TextFormField(
                    controller: _cepController,
                    decoration: const InputDecoration(
                      border: OutlineInputBorder(),
                      hintText: 'Digite seu CEP (ex: 01001-000)',
                      labelText: 'CEP de Entrega',
                    ),
                    keyboardType: TextInputType.number,
                  ),
                  const SizedBox(height: 8.0),
                  SizedBox(
                    width: double.infinity,
                    child: ElevatedButton(
                      style: ElevatedButton.styleFrom(
                        foregroundColor: Colors.white,
                        backgroundColor: Theme.of(context).primaryColor,
                      ),
                      onPressed: _isLoading ? null : _calculateShipping,
                      child: _isLoading
                          ? const SizedBox(
                              height: 20,
                              width: 20,
                              child: CircularProgressIndicator(
                                strokeWidth: 2,
                                color: Colors.white,
                              ),
                            )
                          : const Text('Calcular Frete'),
                    ),
                  ),
                  const SizedBox(height: 4.0),
                  Text(
                    'Frete gratis para compras a partir de R\$ 800,00',
                    style: TextStyle(
                      fontSize: 12.0,
                      color: Colors.grey.shade600,
                    ),
                  ),
                ],
              ),
            ),
          ),
        ],
      ),
    );
  }
}
