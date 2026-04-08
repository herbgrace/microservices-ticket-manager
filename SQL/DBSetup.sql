CREATE DATABASE IF NOT EXISTS UserServiceDB;
USE UserServiceDB;
GO

-- Create tables / Dummy data

CREATE TABLE IF NOT EXISTS [Users] (
    [UserGuid] uniqueidentifier NOT NULL,
    [Username] nvarchar(max) NOT NULL,
    [Email] nvarchar(450) NOT NULL,
    [Password] nvarchar(max) NOT NULL,
    [PasswordHash] nvarchar(max) NOT NULL,
    [CreatedDate] datetime2 NOT NULL DEFAULT (GETUTCDATE()),
    CONSTRAINT [PK_Users] PRIMARY KEY ([UserGuid])
);
GO

INSERT INTO [Users] ([UserGuid], [Username], [Email], [Password], [PasswordHash]) VALUES
("3fb7a13f-edda-48d9-63b3-08de94b76fc3", "herbgrace", "feedback@stardewcroptimizer.com", "woo!!", "AQAAAAIAAYagAAAAEDPowC6yiTak3EDTWN0RAKRASaLm9bu5QGBWIR0cDEM+jLuBx2ac7i0j46fzafas1g==")
("c479d12d-7ad5-416e-63b4-08de94b76fc3", "another user", "fakeEmail@gmail.com", "secure", "AQAAAAIAAYagAAAAEJO3A0WPewLUAlPeyjC7JEVB5MgS2nIFUgRIUv9A6cSzOcOkCNUtU/i07TL26Nmwdw==")