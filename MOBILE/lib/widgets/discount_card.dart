import 'package:flutter/material.dart';
import 'package:scoped_model/scoped_model.dart';

import '../models/cart_model.dart';

class DiscountCard extends StatefulWidget {
  const DiscountCard({super.key});

  @override
  State<DiscountCard> createState() => _DiscountCardState();
}

class _DiscountCardState extends State<DiscountCard> {
  final TextEditingController _couponController =
  TextEditingController();

  bool _isLoading = false;

  @override
  void dispose() {
    _couponController.dispose();
    super.dispose();
  }

  // Validação temporária.
  // Futuramente, esta função será substituída pela chamada à API.
  Future<Map<String, dynamic>?> _validateCoupon(
      String code) async {
    /*
      FUTURA INTEGRAÇÃO COM A API:

      A API deverá receber o código do cupom e consultar o MySQL.

      Ela deverá retornar informações como:
      - código do cupom;
      - percentual de desconto;
      - validade;
      - se o cupom está ativo.

      Exemplo de resposta esperada:
      {
        "valid": true,
        "code": "PROMO10",
        "percent": 10
      }

      Por enquanto, não fazemos consulta ao banco.
    */

    return null;
  }

  Future<void> _applyCoupon() async {
    final code = _couponController.text.trim().toUpperCase();

    if (code.isEmpty) {
      _showMessage('Digite um cupom.');
      return;
    }

    setState(() {
      _isLoading = true;
    });

    try {
      final result = await _validateCoupon(code);

      if (!mounted) return;

      if (result != null && result['valid'] == true) {
        final percent = result['percent'] as int;

        CartModel.of(context).setCoupon(code, percent);

        _showMessage('Cupom aplicado com sucesso!');
      } else {
        CartModel.of(context).setCoupon(null, 0);

        _showMessage('Cupom inválido ou não disponível.');
      }
    } catch (e) {
      if (!mounted) return;

      _showMessage('Não foi possível validar o cupom.');
    } finally {
      if (mounted) {
        setState(() {
          _isLoading = false;
        });
      }
    }
  }

  void _removeCoupon() {
    CartModel.of(context).setCoupon(null, 0);

    _couponController.clear();

    _showMessage('Cupom removido.');
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
    return ScopedModelDescendant<CartModel>(
      builder: (context, child, model) {
        return Card(
          margin: const EdgeInsets.symmetric(
            horizontal: 16,
            vertical: 8,
          ),
          child: ExpansionTile(
            title: const Text('Cupom de desconto'),
            leading: const Icon(Icons.discount),
            children: [
              Padding(
                padding: const EdgeInsets.all(16),
                child: Column(
                  children: [
                    TextFormField(
                      controller: _couponController,
                      textCapitalization:
                      TextCapitalization.characters,
                      decoration: const InputDecoration(
                        hintText: 'Digite seu cupom',
                        border: OutlineInputBorder(),
                      ),
                      enabled: !_isLoading,
                      onFieldSubmitted: (_) => _applyCoupon(),
                    ),
                    const SizedBox(height: 12),
                    if (_isLoading)
                      const CircularProgressIndicator()
                    else
                      Row(
                        children: [
                          Expanded(
                            child: ElevatedButton(
                              onPressed: _applyCoupon,
                              child: const Text('Aplicar'),
                            ),
                          ),
                          if (model.couponCode != null) ...[
                            const SizedBox(width: 8),
                            Expanded(
                              child: OutlinedButton(
                                onPressed: _removeCoupon,
                                child: const Text('Remover'),
                              ),
                            ),
                          ],
                        ],
                      ),
                    if (model.couponCode != null) ...[
                      const SizedBox(height: 8),
                      Text(
                        'Cupom aplicado: ${model.couponCode} '
                            '(${model.discountPercentage}%)',
                      ),
                    ],
                  ],
                ),
              ),
            ],
          ),
        );
      },
    );
  }
}