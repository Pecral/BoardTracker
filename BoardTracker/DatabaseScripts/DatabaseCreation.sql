﻿CREATE TABLE Author(
	AuthorId INT NOT NULL IDENTITY(1,1) PRIMARY KEY,
	Name VARCHAR(50) NOT NULL,
	Lastname VARCHAR(50),
	Description VARCHAR(200)
);

CREATE TABLE Website(
	WebsiteId INT NOT NULL IDENTITY(1,1) PRIMARY KEY,
	Name VARCHAR(50) NOT NULL,
	Url VARCHAR(50) NOT NULL
);

CREATE TABLE Profile(
	ProfileId INT NOT NULL IDENTITY(1,1) PRIMARY KEY,
	AuthorId INT NULL,
	WebsiteId INT NOT NULL,
	Name VARCHAR(50) NOT NULL,
	TemplateKey VARCHAR(50) NOT NULL,
	ProfileUrl VARCHAR(100),

	CONSTRAINT FK_Profile_Author FOREIGN KEY (AuthorId) REFERENCES Author (AuthorId),
	CONSTRAINT FK_Profile_Website FOREIGN KEY (WebsiteId) REFERENCES Website (WebsiteId),
	CONSTRAINT UQ_Profile_NameAndWebsite UNIQUE(TemplateKey, WebsiteId)
);

CREATE TABLE Post(
	PostId INT NOT NULL IDENTITY(1,1) PRIMARY KEY,
	ProfileId INT NOT NULL,
	PostingDateTime DateTime NOT NULL,
	Content VARCHAR(MAX),
	ThreadTitle VARCHAR(500),
	PostLink VARCHAR(500),
	Forum VARCHAR(200),
	ForumLink VARCHAR(300),

	CONSTRAINT FK_Post_Profile FOREIGN KEY (ProfileId) REFERENCES Profile (ProfileId)
);