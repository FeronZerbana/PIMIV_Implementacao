
class ProductData {
  final String category;
  final String id;
  final String title;
  final String description;
  final double price;
  final List<String> imagens;
  final List<String> sizes;

  ProductData({
    required this.category,
    required this.id,
    required this.title,
    required this.description,
    required this.price,
    required this.imagens,
    required this.sizes,
  });

  // Converter os dados recebidos da API em um produto
  factory ProductData.fromMap(Map<String, dynamic> map) {
    return ProductData(
      category: map['category']?.toString() ?? '',
      id: (map['id'] ?? map['product_id'] ?? '').toString(),
      title: map['title']?.toString() ?? '',
      description: map['description']?.toString() ?? '',
      price: double.tryParse(map['price'].toString()) ?? 0.0,
      imagens: List<String>.from(map['imagens'] ?? []),
      sizes: List<String>.from(map['sizes'] ?? []),
    );
  }

  // Converter um produto em mapa
  Map<String, dynamic> toMap() {
    return {
      'category': category,
      'id': id,
      'title': title,
      'description': description,
      'price': price,
      'imagens': imagens,
      'sizes': sizes,
    };
  }

  // Manter o método que você já possuía
  Map<String, dynamic> toResumedMap() {
    return {
      'title': title,
      'description': description,
      'price': price,
    };
  }
}
