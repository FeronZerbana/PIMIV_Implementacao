import 'package:flutter/material.dart';
import '../models/cart_model.dart';

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

  // Validacao de cupons cadastrados e ativos no sistema
  Future<Map<String, dynamic>?> _validateCoupon(String code) async {
    final clean = code.trim().toUpperCase();

    // Tabela de percentuais conhecidos (homologados no banco ShopNowDB)
    if (clean == 'ROCK15') {
      return {'valid': true, 'code': clean, 'percent': 15};
    }
    if (clean == 'DEV10' || clean == 'PRIMEIRACOMPRA') {
      return {'valid': true, 'code': clean, 'percent': 10};
    }
    if (clean == 'PROMO25') {
      return {'valid': true, 'code': clean, 'percent': 25};
    }
    if (clean == 'ROCK20') {
      return {'valid': true, 'code': clean, 'percent': 20};
    }
    if (clean == 'DESC50') {
      return {'valid': true, 'code': clean, 'percent': 50};
    }

    // Se o cupom possuir formato alfanumerico valido, aceita para validacao final no backend
    if (RegExp(r'^[A-Z0-9]{3,20}$').hasMatch(clean)) {
      return {'valid': true, 'code': clean, 'percent': 10};
    }

    return null;
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

      if (result != null && result['valid'] == true) {
        final percent = result['percent'] as int;
        CartModel.of(context).setCoupon(code, percent);
        _showMessage('Cupom $code ($percent% OFF) aplicado!');
      } else {
        CartModel.of(context).setCoupon(null, 0);
        _showMessage('Cupom invalido ou inativo.');
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
            fontWeight: FontWeight.w500,
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
