import '../datas/product_data.dart';
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

    if (response.success && response.data != null) {
      return response.data!;
    }

    // Se nao houver conexao ou a API retornar vazio/erro, nenhum produto deve aparecer
    return <ProductData>[];
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

    if (response.success && response.data != null) {
      return response.data!;
    }

    // Se nao houver conexao ou a API retornar vazio/erro, nenhum produto deve aparecer
    return <ProductData>[];
  }

  static Future<ProductData?> getProductById(int id) async {
    final response = await ApiClient.get<ProductData>(
      '/produtos/$id',
      parser: (json) => ProductData.fromMap(Map<String, dynamic>.from(json as Map)),
    );

    if (response.success && response.data != null) {
      return response.data;
    }

    return null;
  }
}
