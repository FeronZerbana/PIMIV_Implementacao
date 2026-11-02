import 'dart:convert';
import 'package:http/http.dart' as http;
import 'api_config.dart';

class ApiResponse<T> {
  final bool success;
  final T? data;
  final String? message;
  final int statusCode;

  ApiResponse({
    required this.success,
    this.data,
    this.message,
    required this.statusCode,
  });
}

class ApiClient {
  static final http.Client _client = http.Client();
  static const Duration timeout = Duration(seconds: 10);

  static Map<String, String> _headers([String? token]) => {
    'Content-Type': 'application/json; charset=UTF-8',
    'Accept': 'application/json',
    if (token != null) 'Authorization': 'Bearer $token',
  };

  static Future<ApiResponse<T>> get<T>(String endpoint, {T Function(dynamic)? parser, String? token}) async {
    try {
      final url = Uri.parse('${ApiConfig.baseUrl}$endpoint');
      final response = await _client.get(url, headers: _headers(token)).timeout(timeout);
      return _processResponse(response, parser);
    } catch (e) {
      return ApiResponse<T>(success: false, message: 'Falha de conexao com ${ApiConfig.baseUrl}: $e', statusCode: 0);
    }
  }

  static Future<ApiResponse<T>> post<T>(String endpoint, Map<String, dynamic> body, {T Function(dynamic)? parser, String? token}) async {
    try {
      final url = Uri.parse('${ApiConfig.baseUrl}$endpoint');
      final response = await _client.post(url, headers: _headers(token), body: jsonEncode(body)).timeout(timeout);
      return _processResponse(response, parser);
    } catch (e) {
      return ApiResponse<T>(success: false, message: 'Falha de conexao com ${ApiConfig.baseUrl}: $e', statusCode: 0);
    }
  }

  static ApiResponse<T> _processResponse<T>(http.Response response, T Function(dynamic)? parser) {
    dynamic body;
    try {
      body = response.body.isNotEmpty ? jsonDecode(response.body) : null;
    } catch (_) {
      body = response.body;
    }

    if (response.statusCode >= 200 && response.statusCode < 300) {
      return ApiResponse<T>(
        success: true,
        data: parser != null && body != null ? parser(body) : body as T?,
        statusCode: response.statusCode,
      );
    }

    String errorMsg = 'Erro na requisicao';
    if (body is Map && body.containsKey('mensagem')) {
      errorMsg = body['mensagem'].toString();
    } else if (body is Map && body.containsKey('erro')) {
      errorMsg = body['erro'].toString();
    }
    return ApiResponse<T>(success: false, message: errorMsg, statusCode: response.statusCode);
  }
}
