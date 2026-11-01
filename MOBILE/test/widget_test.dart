import 'package:flutter_test/flutter_test.dart';
import 'package:apppim/main.dart';
import 'package:apppim/models/user_model.dart';

void main() {
  testWidgets('App initialization smoke test', (WidgetTester tester) async {
    final userModel = UserModel();
    await tester.pumpWidget(MyApp(userModel: userModel));
    expect(find.byType(MyApp), findsOneWidget);
  });
}
