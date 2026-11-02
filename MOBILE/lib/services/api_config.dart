import 'dart:io';
import 'package:shared_preferences/shared_preferences.dart';

class ApiConfig {
  static const int defaultPort = 5019;
  static const String _prefKey = 'custom_api_url';
  static String? _customUrl;

  static Future<void> init() async {
    final prefs = await SharedPreferences.getInstance();
    _customUrl = prefs.getString(_prefKey);
  }

  static String get defaultBaseUrl {
    if (Platform.isAndroid) {
      return 'http://10.0.2.2:$defaultPort/api';
    }
    return 'http://localhost:$defaultPort/api';
  }

  static String get baseUrl {
    if (_customUrl != null && _customUrl!.trim().isNotEmpty) {
      return _customUrl!.trim();
    }
    return defaultBaseUrl;
  }

  static String get hostServerUrl {
    final current = baseUrl;
    if (current.endsWith('/api')) {
      return current.substring(0, current.length - 4);
    }
    return current;
  }

  static Future<void> setCustomUrl(String? url) async {
    final prefs = await SharedPreferences.getInstance();
    if (url == null || url.trim().isEmpty) {
      _customUrl = null;
      await prefs.remove(_prefKey);
    } else {
      var clean = url.trim();
      if (clean.endsWith('/')) {
        clean = clean.substring(0, clean.length - 1);
      }
      if (!clean.endsWith('/api')) {
        clean = '$clean/api';
      }
      _customUrl = clean;
      await prefs.setString(_prefKey, clean);
    }
  }

  static String formatImageUrl(String? rawUrl) {
    if (rawUrl == null || rawUrl.isEmpty) {
      return '';
    }
    if (rawUrl.startsWith('http://') || rawUrl.startsWith('https://')) {
      return rawUrl;
    }
    final cleanPath = rawUrl.startsWith('/') ? rawUrl : '/$rawUrl';
    return '$hostServerUrl$cleanPath';
  }
}
