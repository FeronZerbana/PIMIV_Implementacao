import '../services/api_config.dart';

class ProductData {
  final String category;
  final String id;
  final String title;
  final String description;
  final double price;
  final List<String> imagens;
  final List<String> sizes;
  final int quantidadeEstoque;
  final String modelo;
  final String rawImageUrl;

  ProductData({
    required this.category,
    required this.id,
    required this.title,
    required this.description,
    required this.price,
    required this.imagens,
    required this.sizes,
    this.quantidadeEstoque = 0,
    this.modelo = '',
    this.rawImageUrl = '',
  });

  // Converter os dados recebidos da API ou mapa em um produto
  factory ProductData.fromMap(Map<String, dynamic> map) {
    final idVal = (map['id'] ?? map['idProduto'] ?? map['product_id'] ?? '').toString();
    final categoria = (map['tipoInstrumento'] ?? map['category'] ?? '').toString();
    final modeloStr = (map['modelo'] ?? '').toString();
    final nomeStr = (map['nome'] ?? map['title'] ?? '').toString();
    final titulo = modeloStr.isNotEmpty ? modeloStr : (nomeStr.isNotEmpty ? nomeStr : categoria);

    final rawImg = (map['imagemUrl'] ?? map['imgURL'] ?? '').toString();
    final List<String> imagensList = [];

    if (rawImg.isNotEmpty) {
      imagensList.add(ApiConfig.formatImageUrl(rawImg));
    } else if (map['imagens'] != null && map['imagens'] is List) {
      imagensList.addAll(List<String>.from(map['imagens']));
    }

    final precoVal = double.tryParse(map['preco']?.toString() ?? map['price']?.toString() ?? '0') ?? 0.0;
    final estoqueVal = int.tryParse(map['quantidadeEstoque']?.toString() ?? map['estoque']?.toString() ?? '0') ?? 0;
    final descVal = (map['descricao'] ?? map['description'] ?? 'Instrumento musical cadastrado no catalogo oficial ShopNow.').toString();

    final List<String> sizesList = map['sizes'] != null && map['sizes'] is List
        ? List<String>.from(map['sizes'])
        : [];

    return ProductData(
      category: categoria,
      id: idVal,
      title: titulo,
      description: descVal,
      price: precoVal,
      imagens: imagensList,
      sizes: sizesList,
      quantidadeEstoque: estoqueVal,
      modelo: modeloStr,
      rawImageUrl: rawImg,
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
      'quantidadeEstoque': quantidadeEstoque,
      'modelo': modelo,
      'rawImageUrl': rawImageUrl,
    };
  }

  Map<String, dynamic> toResumedMap() {
    return {
      'title': title,
      'description': description,
      'price': price,
    };
  }
}
