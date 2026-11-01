import 'package:apppim/datas/product_data.dart';

class CartProduct {
  final String cid;
  final String category;
  final String pid;
  final String? variation;
  int quantity;

  final ProductData productData;

  CartProduct({
    required this.cid,
    required this.category,
    required this.pid,
    required this.productData,
    this.variation,
    this.quantity = 1,
  });

  double get totalPrice => productData.price * quantity;

  Map<String, dynamic> toMap() {
    return {
      'cid': cid,
      'category': category,
      'pid': pid,
      'variation': variation,
      'quantity': quantity,
      'product': {
        'title': productData.title,
        'description': productData.description,
        'price': productData.price,
      },
    };
  }

  Map<String, dynamic> toItemCarrinhoDto() {
    return {
      'codProduto': int.tryParse(pid) ?? 0,
      'quantidade': quantity,
      'precoUnitario': productData.price,
      'nome': productData.title,
    };
  }
}
