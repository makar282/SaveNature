CREATE TABLE [dbo].[AspNetUsers] (
    [Id]                   NVARCHAR (450)     NOT NULL,
    [UserName]             NVARCHAR (256)     NOT NULL,
    [NormalizedUserName]   NVARCHAR (256)     NULL,
    [Email]                NVARCHAR (256)     NULL,
    [NormalizedEmail]      NVARCHAR (256)     NULL,
    [EmailConfirmed]       BIT                NOT NULL,
    [PasswordHash]         NVARCHAR (MAX)     NULL,
    [SecurityStamp]        NVARCHAR (MAX)     NULL,
    [ConcurrencyStamp]     NVARCHAR (MAX)     NULL,
    [PhoneNumber]          NVARCHAR (MAX)     NULL,
    [PhoneNumberConfirmed] BIT                NOT NULL,
    [TwoFactorEnabled]     BIT                NOT NULL,
    [LockoutEnd]           DATETIMEOFFSET (7) NULL,
    [LockoutEnabled]       BIT                NOT NULL,
    [AccessFailedCount]    INT                NOT NULL,
    CONSTRAINT [PK_AspNetUsers] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [AK_AspNetUsers_UserName] UNIQUE NONCLUSTERED ([UserName] ASC)
);


GO
CREATE NONCLUSTERED INDEX [EmailIndex]
    ON [dbo].[AspNetUsers]([NormalizedEmail] ASC);


GO
CREATE UNIQUE NONCLUSTERED INDEX [UserNameIndex]
    ON [dbo].[AspNetUsers]([NormalizedUserName] ASC) WHERE ([NormalizedUserName] IS NOT NULL);


CREATE TABLE [dbo].[Receipts] (
    [Id]           INT             IDENTITY (1, 1) NOT NULL,
    [UserName]     NVARCHAR (256)  NOT NULL,
    [ReceiptData]  NVARCHAR (MAX)  NOT NULL,
    [TotalAmount]  DECIMAL (18, 2) NOT NULL,
    [PurchaseDate] DATETIME2 (7)   NOT NULL,
    [CreatedAt]    DATETIME2 (7)   NOT NULL,
    CONSTRAINT [PK_Receipts] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_Receipts_AspNetUsers_UserName] FOREIGN KEY ([UserName]) REFERENCES [dbo].[AspNetUsers] ([UserName])
);


GO
CREATE NONCLUSTERED INDEX [IX_Receipts_UserName]
    ON [dbo].[Receipts]([UserName] ASC);



CREATE TABLE [dbo].[Items] (
    [Id]          INT             IDENTITY (1, 1) NOT NULL,
    [ReceiptId]   INT             NOT NULL,
    [ProductName] NVARCHAR (MAX)  NOT NULL,
    [Price]       DECIMAL (18, 2) NOT NULL,
    [EcoScore]    INT             NOT NULL,
    CONSTRAINT [PK_Items] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_Items_Receipts_ReceiptId] FOREIGN KEY ([ReceiptId]) REFERENCES [dbo].[Receipts] ([Id]) ON DELETE CASCADE
);


GO
CREATE NONCLUSTERED INDEX [IX_Items_ReceiptId]
    ON [dbo].[Items]([ReceiptId] ASC);

CREATE TABLE [dbo].[Recommendations] (
    [Id]                 INT            IDENTITY (1, 1) NOT NULL,
    [Purchase]           NVARCHAR (MAX) NOT NULL,
    [RecommendationText] NVARCHAR (MAX) NOT NULL
);