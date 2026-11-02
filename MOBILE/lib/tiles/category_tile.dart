
import 'package:flutter/material.dart';
import '../screens/category_screen.dart';

class CategoryTile extends StatelessWidget {
  final String title;
  final String imagePath;

  const CategoryTile({
    super.key,
    required this.title,
    required this.imagePath,
  });

  @override
  Widget build(BuildContext context) {
    return ListTile(
      leading: CircleAvatar(
        radius: 25,
        backgroundColor: Colors.transparent,
        child: ClipOval(
          child: Image.asset(
            imagePath,
            width: 50,
            height: 50,
            fit: BoxFit.cover,
            errorBuilder: (context, error, stackTrace) {
              return const Icon(
                Icons.broken_image,
                color: Colors.red,
              );
            },
          ),
        ),
      ),
      title: Text(title),
      trailing: const Icon(Icons.keyboard_arrow_right),
      onTap: () {
        Navigator.of(context).push(
          MaterialPageRoute(
            builder: (context) => CategoryScreen(title),
          ),
        );
      },
    );
  }
}

