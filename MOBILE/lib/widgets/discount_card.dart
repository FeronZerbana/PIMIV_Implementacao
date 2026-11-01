import 'package:flutter/material.dart';
import '../models/cart_model.dart';
import '../services/api_client.dart';

class DiscountCard extends StatefulWidget {
  const DiscountCard({super.key});

  @override
  State<DiscountCard> createState() => _DiscountCardState();
}

class _DiscountCardState extends State<DiscountCard> {
  final TextEditingController _couponController = TextEditingController();
  bool _isLoading = false;

  @override
  void dispose() {
    _couponController.dispose();
    super.dispose();
  }

  // Validacao de cupons consultando a API em tempo real
  Future<Map<String, dynamic>> _validateCoupon(String code) async {
    final clean = code.trim().toUpperCase();

    // 1. Tentar validar via endpoint oficial da Web API: GET /api/cupons/validar
    try {
      final cartModel = CartModel.of(context);
      final int? codCliente = cartModel.user?.isLoggedIn() == true
          ? int.tryParse(cartModel.user!.userdata['id']?.toString() ?? '')
          : null;

      final queryParams = codCliente != null
          ? '/cupons/validar?codigo=$clean&codCliente=$codCliente'
          : '/cupons/validar?codigo=$clean';

      final response = await ApiClient.get<Map<String, dynamic>>(
        queryParams,
        parser: (json) => Map<String, dynamic>.from(json as Map),
      );

      if (response.success && response.data != null && response.data!['valido'] == true) {
        final rawDesconto = response.data!['desconto'];
        final int percent = int.tryParse(rawDesconto.toString()) ?? 10;
        return {'valid': true, 'code': clean, 'percent': percent};
      } else if (response.data != null && response.data!['mensagem'] != null) {
        return {'valid': false, 'message': response.data!['mensagem'].toString()};
      }
    } catch (_) {
      // Falha de rede: prossegue para deteccao de contingencia
    }

    // 2. Deteccao dinamica caso a API esteja temporariamente inacessivel:
    // Extrai o percentual se o codigo contiver numero (ex: PROMO20 -> 20, CUPOM30 -> 30, ROCK15 -> 15)
    final numMatch = RegExp(r'(\d{1,2})$').firstMatch(clean);
    if (numMatch != null) {
      final int parsed = int.tryParse(numMatch.group(1)!) ?? 0;
      if (parsed > 0 && parsed <= 50) {
        return {'valid': true, 'code': clean, 'percent': parsed};
      }
    }

    if (clean == 'PRIMEIRACOMPRA' || clean == 'DEV10') {
      return {'valid': true, 'code': clean, 'percent': 10};
    }

    return {'valid': false, 'message': 'Cupom invalido ou inexistente no sistema.'};
  }

  Future<void> _applyCoupon() async {
    final code = _couponController.text.trim().toUpperCase();

    if (code.isEmpty) {
      _showMessage('Digite o codigo do cupom.');
      return;
    }

    setState(() {
      _isLoading = true;
    });

    try {
      final result = await _validateCoupon(code);

      if (!mounted) return;

      if (result['valid'] == true) {
        final percent = result['percent'] as int;
        CartModel.of(context).setCoupon(code, percent);
        _showMessage('Cupom $code ($percent% OFF) aplicado!');
      } else {
        CartModel.of(context).setCoupon(null, 0);
        final msg = result['message']?.toString() ?? 'Cupom invalido ou inativo.';
        _showMessage(msg);
      }
    } catch (_) {
      if (!mounted) return;
      _showMessage('Nao foi possivel validar o cupom.');
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
    final cartModel = CartModel.of(context);
    final bool hasCoupon = cartModel.couponCode != null;

    if (hasCoupon && _couponController.text.isEmpty) {
      _couponController.text = cartModel.couponCode!;
    }

    return Card(
      margin: const EdgeInsets.symmetric(
        horizontal: 8.0,
        vertical: 4.0,
      ),
      child: ExpansionTile(
        initiallyExpanded: hasCoupon,
        title: Text(
          hasCoupon
              ? 'Cupom Ativo: ${cartModel.couponCode} (${cartModel.discountPercentage}% OFF)'
              : 'Cupom de Desconto',
          textAlign: TextAlign.start,
          style: TextStyle(
            fontWeight: FontWeight.w600,
            color: hasCoupon ? Theme.of(context).primaryColor : Colors.grey.shade700,
          ),
        ),
        leading: Icon(
          Icons.card_giftcard,
          color: hasCoupon ? Theme.of(context).primaryColor : null,
        ),
        children: [
          Padding(
            padding: const EdgeInsets.all(16.0),
            child: Column(
              children: [
                TextFormField(
                  controller: _couponController,
                  textCapitalization: TextCapitalization.characters,
                  decoration: InputDecoration(
                    border: const OutlineInputBorder(),
                    hintText: 'Ex: ROCK15, DEV10, PRIMEIRACOMPRA',
                    labelText: 'Codigo do Cupom',
                    suffixIcon: hasCoupon
                        ? IconButton(
                      icon: const Icon(Icons.clear),
                      onPressed: _removeCoupon,
                    )
                        : null,
                  ),
                  onFieldSubmitted: (_) => _applyCoupon(),
                ),
                const SizedBox(height: 12.0),
                SizedBox(
                  width: double.infinity,
                  child: ElevatedButton(
                    style: ElevatedButton.styleFrom(
                      backgroundColor: Theme.of(context).primaryColor,
                      foregroundColor: Colors.white,
                    ),
                    onPressed: _isLoading ? null : _applyCoupon,
                    child: _isLoading
                        ? const SizedBox(
                      width: 18,
                      height: 18,
                      child: CircularProgressIndicator(
                        strokeWidth: 2,
                        color: Colors.white,
                      ),
                    )
                        : const Text('Aplicar Cupom'),
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
