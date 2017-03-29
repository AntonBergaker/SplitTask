CREATE TABLE Users (
	UserID int NOT NULL,
	Username nvarchar(255) NOT NULL,
	PasswordHash BINARY(64) NOT NULL,
	DisplayName nvarchar(255),
	Email nvarchar(255) NOT NULL,

	PRIMARY KEY(UserID)
);

CREATE TABLE UserLists (
	UserID int NOT NULL,
	ListID int NOT NULL
);