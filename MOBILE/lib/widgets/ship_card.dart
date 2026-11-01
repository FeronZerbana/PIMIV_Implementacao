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

    // Simulacao de tarifario regional por faixa de CEP
    double calculated = 19.90;
    final digits = cep.replaceAll(RegExp(r'\D'), '');
    if (digits.startsWith('0')) {
      calculated = 12.50; // Regiao Metropolitana SP
    } else if (digits.startsWith('1')) {
      calculated = 18.00; // Interior SP
    } else {
      calculated = 28.90; // Demais Estados
    }

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
          : 'Frete calculado: R\$ ${calculated.toStringAsFixed(2)}',
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
              ? 'Frete: R\$ ${cartModel.shipPrice.toStringAsFixed(2)} ($_calculatedCep)'
              : 'Calcular Frete e Entrega',
          textAlign: TextAlign.start,
          style: TextStyle(
            fontWeight: FontWeight.w500,
            color: _calculatedCep != null ? Theme.of(context).primaryColor : Colors.grey.shade700,
          ),
        ),
        leading: Icon(
          Icons.location_on,
          color: _calculatedCep != null ? Theme.of(context).primaryColor : null,
        ),
        children: [
          Padding(
            padding: const EdgeInsets.all(16.0),
            child: Column(
              children: [
                TextFormField(
                  controller: _cepController,
                  keyboardType: TextInputType.number,
                  maxLength: 9,
                  decoration: const InputDecoration(
                    border: OutlineInputBorder(),
                    hintText: '01001-000',
                    labelText: 'CEP de Entrega',
                    counterText: '',
                  ),
                  onFieldSubmitted: (_) => _calculateShipping(),
                ),
                const SizedBox(height: 12.0),
                SizedBox(
                  width: double.infinity,
                  child: ElevatedButton(
                    style: ElevatedButton.styleFrom(
                      backgroundColor: Theme.of(context).primaryColor,
                      foregroundColor: Colors.white,
                    ),
                    onPressed: _isLoading ? null : _calculateShipping,
                    child: _isLoading
                        ? const SizedBox(
                      width: 18,
                      height: 18,
                      child: CircularProgressIndicator(strokeWidth: 2, color: Colors.white),
                    )
                        : const Text('Calcular Frete'),
                  ),
                ),
              ],
            ),
          ),
        ],
      ),
    );
  }
}
