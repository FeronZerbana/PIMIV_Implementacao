
import 'package:flutter/material.dart';

class ShipCard extends StatefulWidget {
  const ShipCard({super.key});

  @override
  State<ShipCard> createState() => _ShipCardState();
}

class _ShipCardState extends State<ShipCard> {
  final TextEditingController _cepController =
  TextEditingController();

  bool _isLoading = false;
  String? _cep;

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
      _showMessage('Digite um CEP válido.');
      return;
    }

    setState(() {
      _isLoading = true;
      _cep = cep;
    });

    // Futuramente, a API receberá o CEP,
    // consultará o serviço de frete e retornará o valor.

    setState(() {
      _isLoading = false;
    });

    _showMessage('CEP informado com sucesso!');
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
    return Card(
      margin: const EdgeInsets.symmetric(
        horizontal: 8.0,
        vertical: 4.0,
      ),
      child: ExpansionTile(
        title: const Text(
          'Calcular Frete',
          textAlign: TextAlign.start,
          style: TextStyle(
            fontWeight: FontWeight.w500,
            color: Colors.grey,
          ),
        ),
        leading: const Icon(Icons.location_on),
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
                    hintText: 'Digite seu CEP',
                    counterText: '',
                  ),
                  onFieldSubmitted: (_) {
                    _calculateShipping();
                  },
                ),
                const SizedBox(height: 12.0),
                SizedBox(
                  width: double.infinity,
                  child: ElevatedButton(
                    onPressed:
                    _isLoading ? null : _calculateShipping,
                    child: const Text('Calcular'),
                  ),
                ),
                if (_isLoading)
                  const CircularProgressIndicator(),
                if (_cep != null) ...[
                  const SizedBox(height: 8.0),
                  Text('CEP informado: $_cep'),
                ],
              ],
            ),
          ),
        ],
      ),
    );
  }
}
