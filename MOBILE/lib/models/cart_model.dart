
import 'package:scoped_model/scoped_model.dart';

import '../datas/cart_product.dart';

class CartModel extends Model {
  // Produtos adicionados ao carrinho.
  final List<CartProduct> products = [];

  // Estado de carregamento.
  bool isLoading = false;

  // Dados do cupom de desconto.
  String? couponCode;
  int discountPercentage = 0;

  // Valor fixo de frete provisório.
  // Futuramente será calculado pela API.
  double shipPrice = 9.99;

  // Obtém o CartModel através do ScopedModel.
  static CartModel of(context) {
    return ScopedModel.of<CartModel>(context);
  }

  // Adiciona um produto ao carrinho.
  void addCartItem(CartProduct cartProduct) {
    products.add(cartProduct);
    updatePrices();
  }

  // Remove um produto do carrinho.
  void removeCartItem(CartProduct cartProduct) {
    products.removeWhere(
          (item) => item.cid == cartProduct.cid,
    );

    if (products.isEmpty) {
      couponCode = null;
      discountPercentage = 0;
    }

    updatePrices();
  }

  // Atualiza a quantidade de um produto.
  void updateQuantity(
      CartProduct cartProduct,
      int quantity,
      ) {
    if (quantity <= 0) {
      removeCartItem(cartProduct);
      return;
    }

    cartProduct.quantity = quantity;
    updatePrices();
  }

  // Aumenta a quantidade de um produto.
  void incProduct(CartProduct cartProduct) {
    if (!products.contains(cartProduct)) return;

    cartProduct.quantity++;
    updatePrices();
  }

  // Diminui a quantidade de um produto.
  void decProduct(CartProduct cartProduct) {
    if (!products.contains(cartProduct)) return;

    if (cartProduct.quantity > 1) {
      cartProduct.quantity--;
      updatePrices();
    } else {
      removeCartItem(cartProduct);
    }
  }

  // Aplica ou remove um cupom de desconto.
  void setCoupon(
      String? couponCode,
      int discountPercentage,
      ) {
    final int validPercentage =
    discountPercentage.clamp(0, 100);

    if (products.isEmpty ||
        couponCode == null ||
        couponCode.trim().isEmpty ||
        validPercentage == 0) {
      this.couponCode = null;
      this.discountPercentage = 0;
    } else {
      this.couponCode = couponCode.trim().toUpperCase();
      this.discountPercentage = validPercentage;
    }

    updatePrices();
  }

  // Remove o cupom aplicado.
  void removeCoupon() {
    couponCode = null;
    discountPercentage = 0;

    updatePrices();
  }

  // Limpa todos os produtos do carrinho.
  void clearCart() {
    products.clear();

    couponCode = null;
    discountPercentage = 0;

    updatePrices();
  }

  // Substitui os produtos do carrinho.
  void setCartItems(List<CartProduct> items) {
    products
      ..clear()
      ..addAll(items);

    if (products.isEmpty) {
      couponCode = null;
      discountPercentage = 0;
    }

    updatePrices();
  }

  // Quantidade total de unidades no carrinho.
  int get itemCount {
    return products.fold(
      0,
          (total, item) => total + item.quantity,
    );
  }

  // Subtotal dos produtos.
  double get totalPrice {
    return getProductsPrice();
  }

  // Calcula o subtotal dos produtos.
  double getProductsPrice() {
    double price = 0.0;

    for (CartProduct cartProduct in products) {
      price += cartProduct.quantity *
          cartProduct.productData.price;
    }

    return price;
  }

  // Retorna o valor do frete.
  // Valor provisório até desenvolver o cálculo real.
  double getShipPrice() {
    return shipPrice;
  }

  // Calcula o valor do desconto.
  double getDiscount() {
    return getProductsPrice() *
        discountPercentage /
        100;
  }

  // Valor do desconto aplicado.
  double get discountAmount {
    return getDiscount();
  }

  // Total dos produtos após aplicar o desconto.
  double get totalWithDiscount {
    return getProductsPrice() - getDiscount();
  }

  // Total final, incluindo o frete.
  double get totalPriceWithShipping {
    return totalWithDiscount + getShipPrice();
  }

  // Atualiza os widgets que dependem do CartModel.
  void updatePrices() {
    notifyListeners();
  }

  // Finalizar pedido.
  // A integração com a API será feita posteriormente.
  Future<String?> finishOrder() async {
    if (products.isEmpty || isLoading) {
      return null;
    }

    isLoading = true;
    notifyListeners();

    try {
      // FUTURA INTEGRAÇÃO COM A API:
      //
      // 1. Enviar os dados do pedido.
      // 2. A API validará os valores e o cupom.
      // 3. A API calculará/confirmará o frete.
      // 4. A API salvará o pedido no MySQL.
      // 5. A API retornará o ID do pedido.
      // 6. O carrinho será limpo após a confirmação.

      return null;
    } catch (e) {
      rethrow;
    } finally {
      isLoading = false;
      notifyListeners();
    }
  }
}
