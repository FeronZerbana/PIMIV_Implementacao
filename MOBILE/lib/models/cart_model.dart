import 'package:flutter/material.dart';
import 'package:scoped_model/scoped_model.dart';
import '../datas/cart_product.dart';
import '../models/user_model.dart';
import '../services/order_service.dart';

class CartModel extends Model {
  UserModel? user;

  CartModel([this.user]);

  final List<CartProduct> products = [];
  bool isLoading = false;
  String? lastOrderError;

  String? couponCode;
  int discountPercentage = 0;

  double shipPrice = 9.99;

  static CartModel of(BuildContext context) {
    return ScopedModel.of<CartModel>(context);
  }

  void updateUser(UserModel newUser) {
    user = newUser;
  }

  void setShippingPrice(double newPrice) {
    shipPrice = newPrice >= 0 ? newPrice : 0.0;
    updatePrices();
  }

  void addCartItem(CartProduct cartProduct) {
    final existingIndex = products.indexWhere((item) => item.pid == cartProduct.pid);
    if (existingIndex >= 0) {
      products[existingIndex].quantity += cartProduct.quantity;
    } else {
      products.add(cartProduct);
    }
    updatePrices();
  }

  void removeCartItem(CartProduct cartProduct) {
    products.removeWhere((item) => item.cid == cartProduct.cid || item.pid == cartProduct.pid);

    if (products.isEmpty) {
      couponCode = null;
      discountPercentage = 0;
    }

    updatePrices();
  }

  void updateQuantity(CartProduct cartProduct, int quantity) {
    if (quantity <= 0) {
      removeCartItem(cartProduct);
      return;
    }

    cartProduct.quantity = quantity;
    updatePrices();
  }

  void incProduct(CartProduct cartProduct) {
    if (!products.contains(cartProduct)) return;

    if (cartProduct.productData.quantidadeEstoque > 0 &&
        cartProduct.quantity >= cartProduct.productData.quantidadeEstoque) {
      return;
    }

    cartProduct.quantity++;
    updatePrices();
  }

  void decProduct(CartProduct cartProduct) {
    if (!products.contains(cartProduct)) return;

    if (cartProduct.quantity > 1) {
      cartProduct.quantity--;
      updatePrices();
    } else {
      removeCartItem(cartProduct);
    }
  }

  void setCoupon(String? code, int percent) {
    final int validPercentage = percent.clamp(0, 100);

    if (products.isEmpty || code == null || code.trim().isEmpty || validPercentage == 0) {
      couponCode = null;
      discountPercentage = 0;
    } else {
      couponCode = code.trim().toUpperCase();
      discountPercentage = validPercentage;
    }

    updatePrices();
  }

  void removeCoupon() {
    couponCode = null;
    discountPercentage = 0;
    updatePrices();
  }

  void clearCart() {
    products.clear();
    couponCode = null;
    discountPercentage = 0;
    updatePrices();
  }

  int get itemCount {
    return products.fold(0, (total, item) => total + item.quantity);
  }

  double get totalPrice {
    return getProductsPrice();
  }

  double getProductsPrice() {
    double price = 0.0;
    for (CartProduct cartProduct in products) {
      price += cartProduct.quantity * cartProduct.productData.price;
    }
    return price;
  }

  double getShipPrice() {
    return shipPrice;
  }

  double getDiscount() {
    return getProductsPrice() * discountPercentage / 100;
  }

  double get discountAmount {
    return getDiscount();
  }

  double get totalWithDiscount {
    final subtotal = getProductsPrice();
    final discount = getDiscount();
    return subtotal > discount ? subtotal - discount : 0.0;
  }

  double get totalPriceWithShipping {
    return totalWithDiscount + getShipPrice();
  }

  void updatePrices() {
    notifyListeners();
  }

  Future<String?> finishOrder() async {
    if (products.isEmpty || isLoading) {
      return null;
    }

    final int? codCliente = user?.idCliente;
    if (codCliente == null || codCliente <= 0) {
      lastOrderError = 'E necessario estar autenticado para concluir a compra.';
      notifyListeners();
      return null;
    }

    isLoading = true;
    lastOrderError = null;
    notifyListeners();

    try {
      final itensDto = products.map((p) => p.toItemCarrinhoDto()).toList();

      final response = await OrderService.finishOrder(
        codCliente: codCliente,
        itens: itensDto,
        cupom: couponCode,
      );

      if (response.success && response.data != null && response.data! > 0) {
        final orderId = response.data!.toString();
        clearCart();
        return orderId;
      } else {
        lastOrderError = response.message ?? 'Falha ao registrar pedido no servidor.';
        return null;
      }
    } catch (e) {
      lastOrderError = 'Erro ao conectar ao servidor: $e';
      return null;
    } finally {
      isLoading = false;
      notifyListeners();
    }
  }
}
