-- Voer dit uit in phpMyAdmin of MySQL voordat je de app start
-- Voegt een username kolom toe aan de users tabel

ALTER TABLE `users`
  ADD COLUMN `username` VARCHAR(50) NULL UNIQUE AFTER `lastname`;

-- Vul bestaande gebruikers in met een standaard username (voornaam + id)
UPDATE `users` SET `username` = CONCAT(LOWER(firstname), id) WHERE username IS NULL;

-- Maak username verplicht na de migratie
ALTER TABLE `users`
  MODIFY COLUMN `username` VARCHAR(50) NOT NULL;
