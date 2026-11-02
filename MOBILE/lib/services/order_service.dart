import 'api_client.dart';

class OrderService {
  static Future<ApiResponse<int>> finishOrder({
    required int codCliente,
    required List<Map<String, dynamic>> itens,
    String? cupom,
    double? valorFrete,
  }) async {
    final body = {
      'codCliente': codCliente,
      'itens': itens,
      'cupom': cupom != null && cupom.trim().isNotEmpty ? cupom.trim().toUpperCase() : null,
      'valorFrete': valorFrete,
    };

    return await ApiClient.post<int>(
      '/vendas/finalizar',
      body,
      parser: (json) {
        if (json is Map && json.containsKey('vendaID')) {
          return int.tryParse(json['vendaID'].toString()) ?? 0;
        }
        return 0;
      },
    );
  }

  static Future<List<Map<String, dynamic>>> getCustomerOrders(int codCliente) async {
    final response = await ApiClient.get<List<Map<String, dynamic>>>(
      '/vendas/cliente/$codCliente',
      parser: (json) {
        if (json is List) {
          return json.map((i) => Map<String, dynamic>.from(i as Map)).toList();
        }
        return <Map<String, dynamic>>[];
      },
    );

    if (response.success && response.data != null) {
      return response.data!;
    }

    return [];
  }
}
