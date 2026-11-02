import 'api_client.dart';

class AuthService {
  static Future<ApiResponse<Map<String, dynamic>>> login(String email, String senha) async {
    return await ApiClient.post<Map<String, dynamic>>(
      '/auth/login',
      {
        'email': email.trim(),
        'senha': senha,
      },
      parser: (json) => Map<String, dynamic>.from(json as Map),
    );
  }

  static Future<ApiResponse<Map<String, dynamic>>> register({
    required String nome,
    required String email,
    required String senha,
    String? telefone,
  }) async {
    return await ApiClient.post<Map<String, dynamic>>(
      '/auth/registrar',
      {
        'nome': nome.trim(),
        'email': email.trim().toLowerCase(),
        'senha': senha,
        'telefone': telefone?.trim() ?? '',
      },
      parser: (json) => Map<String, dynamic>.from(json as Map),
    );
  }
}
