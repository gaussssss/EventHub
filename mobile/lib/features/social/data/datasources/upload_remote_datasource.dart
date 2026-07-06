import 'package:dio/dio.dart';
import '../../../../core/network/api_client.dart';

/// Upload d'images vers l'API (`POST /api/uploads/image`, multipart « file »).
class UploadRemoteDataSource {
  final ApiClient _client;

  UploadRemoteDataSource(this._client);

  /// Envoie le fichier local et renvoie l'URL absolue servie par l'API.
  Future<String> uploadImage(String filePath) async {
    final formData = FormData.fromMap({
      'file': await MultipartFile.fromFile(filePath),
    });
    final data = await _client.post('/api/uploads/image', data: formData);
    return (data as Map<String, dynamic>)['url'] as String;
  }
}
