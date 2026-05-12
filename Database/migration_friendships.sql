-- Voer dit uit in phpMyAdmin

CREATE TABLE IF NOT EXISTS `friendships` (
  `id`         int(11) NOT NULL AUTO_INCREMENT,
  `sender_id`  int(11) NOT NULL,
  `receiver_id`int(11) NOT NULL,
  `status`     enum('pending','accepted','declined') NOT NULL DEFAULT 'pending',
  `created_at` datetime DEFAULT current_timestamp(),
  PRIMARY KEY (`id`),
  UNIQUE KEY `unique_pair` (`sender_id`,`receiver_id`),
  KEY `sender_id`   (`sender_id`),
  KEY `receiver_id` (`receiver_id`),
  CONSTRAINT `f_sender`   FOREIGN KEY (`sender_id`)   REFERENCES `users` (`id`) ON DELETE CASCADE,
  CONSTRAINT `f_receiver` FOREIGN KEY (`receiver_id`) REFERENCES `users` (`id`) ON DELETE CASCADE
);

-- Groups tabel uitbreiden met chat-ondersteuning
CREATE TABLE IF NOT EXISTS `group_messages` (
  `id`         int(11) NOT NULL AUTO_INCREMENT,
  `group_id`   int(11) NOT NULL,
  `sender_id`  int(11) NOT NULL,
  `message`    text NOT NULL,
  `sent_at`    datetime DEFAULT current_timestamp(),
  PRIMARY KEY (`id`),
  KEY `group_id`  (`group_id`),
  KEY `sender_id` (`sender_id`),
  CONSTRAINT `gm_group`  FOREIGN KEY (`group_id`)  REFERENCES `groups` (`id`) ON DELETE CASCADE,
  CONSTRAINT `gm_sender` FOREIGN KEY (`sender_id`) REFERENCES `users`  (`id`) ON DELETE CASCADE
);
