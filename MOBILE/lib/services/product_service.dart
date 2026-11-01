import '../datas/product_data.dart';
import '../data/store_data.dart' as fallback;
import 'api_client.dart';

class ProductService {
  static Future<List<ProductData>> getProducts() async {
    final response = await ApiClient.get<List<ProductData>>(
      '/produtos',
      parser: (json) {
        if (json is List) {
          return json
              .map((item) => ProductData.fromMap(Map<String, dynamic>.from(item as Map)))
              .toList();
        }
        return <ProductData>[];
      },
    );

    if (response.success && response.data != null && response.data!.isNotEmpty) {
      return response.data!;
    }

    // Fallback de contingencia caso o backend esteja temporariamente inacessivel
    return fallback.products;
  }

  static Future<List<ProductData>> getProductsByCategory(String categoria) async {
    final response = await ApiClient.get<List<ProductData>>(
      '/produtos/categoria/$categoria',
      parser: (json) {
        if (json is List) {
          return json
              .map((item) => ProductData.fromMap(Map<String, dynamic>.from(item as Map)))
              .toList();
        }
        return <ProductData>[];
      },
    );

    if (response.success && response.data != null && response.data!.isNotEmpty) {
      return response.data!;
    }

    // Fallback filtrando produtos locais
    return fallback.products.where((p) {
      return p.category.toLowerCase() == categoria.toLowerCase();
    }).toList();
  }

  static Future<ProductData?> getProductById(int id) async {
    final response = await ApiClient.get<ProductData>(
      '/produtos/$id',
      parser: (json) => ProductData.fromMap(Map<String, dynamic>.from(json as Map)),
    );

    if (response.success && response.data != null) {
      return response.data;
    }

    try {
      return fallback.products.firstWhere((p) => p.id == id.toString());
    } catch (_) {
      return null;
    }
  }
}
