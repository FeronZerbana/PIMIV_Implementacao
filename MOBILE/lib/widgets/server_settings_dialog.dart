import 'package:flutter/material.dart';
import '../services/api_config.dart';
import '../services/api_client.dart';

class ServerSettingsDialog extends StatefulWidget {
  const ServerSettingsDialog({super.key});

  static Future<void> show(BuildContext context) {
    return showDialog(
      context: context,
      builder: (_) => const ServerSettingsDialog(),
    );
  }

  @override
  State<ServerSettingsDialog> createState() => _ServerSettingsDialogState();
}

class _ServerSettingsDialogState extends State<ServerSettingsDialog> {
  late final TextEditingController _controller;
  bool _testing = false;
  String? _statusMessage;
  bool _isSuccess = false;

  @override
  void initState() {
    super.initState();
    _controller = TextEditingController(text: ApiConfig.baseUrl);
  }

  @override
  void dispose() {
    _controller.dispose();
    super.dispose();
  }

  Future<void> _testConnection() async {
    setState(() {
      _testing = true;
      _statusMessage = null;
    });

    await ApiConfig.setCustomUrl(_controller.text);
    final response = await ApiClient.get<List<dynamic>>('/produtos');

    if (!mounted) return;
    setState(() {
      _testing = false;
      if (response.success) {
        final list = response.data ?? [];
        _isSuccess = true;
        _statusMessage = 'Conectado com sucesso! (${list.length} produtos carregados)';
      } else {
        _isSuccess = false;
        _statusMessage = 'Falha: ${response.message ?? "Servidor nao respondeu."}';
      }
    });
  }

  Future<void> _resetDefault() async {
    await ApiConfig.setCustomUrl(null);
    setState(() {
      _controller.text = ApiConfig.baseUrl;
      _statusMessage = 'Restaurado para o padrao local da plataforma.';
      _isSuccess = true;
    });
  }

  @override
  Widget build(BuildContext context) {
    final primaryColor = Theme.of(context).primaryColor;

    return AlertDialog(
      title: const Row(
        children: [
          Icon(Icons.dns, size: 24),
          SizedBox(width: 8),
          Text('Configuração do servidor'),
        ],
      ),
      content: SingleChildScrollView(
        child: Column(
          mainAxisSize: MainAxisSize.min,
          crossAxisAlignment: CrossAxisAlignment.start,
          children: [
            const Text(
              'Defina o endereco da API para testes com Dev Tunnels, Emulador ou Rede Wi-Fi:',
              style: TextStyle(fontSize: 13),
            ),
            const SizedBox(height: 12),
            TextField(
              controller: _controller,
              decoration: const InputDecoration(
                border: OutlineInputBorder(),
                labelText: 'URL Base da API',
                hintText: 'http://10.0.2.2:5019/api',
              ),
            ),
            const SizedBox(height: 8),
            Text(
              'Padrao da plataforma: ${ApiConfig.defaultBaseUrl}',
              style: const TextStyle(fontSize: 11, color: Colors.grey),
            ),
            if (_statusMessage != null) ...[
              const SizedBox(height: 12),
              Container(
                padding: const EdgeInsets.all(8),
                decoration: BoxDecoration(
                  color: _isSuccess ? Colors.green.shade50 : Colors.red.shade50,
                  borderRadius: BorderRadius.circular(4),
                  border: Border.all(
                    color: _isSuccess ? Colors.green : Colors.red,
                    width: 1,
                  ),
                ),
                child: Text(
                  _statusMessage!,
                  style: TextStyle(
                    fontSize: 12,
                    color: _isSuccess ? Colors.green.shade900 : Colors.red.shade900,
                    fontWeight: FontWeight.bold,
                  ),
                ),
              ),
            ],
          ],
        ),
      ),
      actions: [
        TextButton(
          onPressed: _resetDefault,
          child: const Text('Restaurar Padrao'),
        ),
        ElevatedButton(
          style: ElevatedButton.styleFrom(
            backgroundColor: primaryColor,
            foregroundColor: Colors.white,
          ),
          onPressed: _testing ? null : _testConnection,
          child: _testing
              ? const SizedBox(
            width: 16,
            height: 16,
            child: CircularProgressIndicator(
              strokeWidth: 2,
              color: Colors.white,
            ),
          )
              : const Text('Testar e Salvar'),
        ),
      ],
    );
  }
}
