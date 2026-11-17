SET NOCOUNT ON;
SET XACT_ABORT ON;

DECLARE @Apply tinyint = TRY_CONVERT(tinyint, N'$(APPLY)');
DECLARE @ImportedAt datetimeoffset(7) = TODATETIMEOFFSET(SYSUTCDATETIME(), N'+00:00');
DECLARE @AuditActor nvarchar(450) = N'System';

IF @Apply IS NULL OR @Apply NOT IN (0, 1)
    THROW 51000, N'APPLY must be supplied through sqlcmd as either 0 or 1.', 1;

DECLARE @Import TABLE
(
    ImportOrder int NOT NULL,
    PersonId uniqueidentifier NOT NULL,
    DisplayName nvarchar(200) NOT NULL,
    NormalizedName nvarchar(400) NOT NULL,
    BaseWebsiteUrl nvarchar(2048) NOT NULL,
    NormalizedWebsiteHost nvarchar(253) NOT NULL,
    AllowedHost1 nvarchar(253) NOT NULL,
    AllowedHost2 nvarchar(253) NOT NULL,
    RuleCode nvarchar(32) NOT NULL,
    RuleName nvarchar(200) NOT NULL,
    RuleType nvarchar(24) NOT NULL,
    JsonLdObjectType nvarchar(200) NULL,
    JsonLdPricePath nvarchar(512) NULL,
    JsonLdCurrencyPath nvarchar(512) NULL,
    CssSelector nvarchar(2048) NULL,
    CssValueSource nvarchar(24) NULL,
    CssAttributeName nvarchar(200) NULL,
    DecimalSeparator nvarchar(16) NOT NULL,
    ThousandsSeparator nvarchar(16) NOT NULL,
    ExpectedCurrencyCode nvarchar(3) NOT NULL,
    MinimumValue decimal(18, 2) NOT NULL,
    MaximumValue decimal(18, 2) NOT NULL,
    TestUrl nvarchar(2048) NOT NULL
);

INSERT INTO @Import
(
    ImportOrder,
    PersonId,
    DisplayName,
    NormalizedName,
    BaseWebsiteUrl,
    NormalizedWebsiteHost,
    AllowedHost1,
    AllowedHost2,
    RuleCode,
    RuleName,
    RuleType,
    JsonLdObjectType,
    JsonLdPricePath,
    JsonLdCurrencyPath,
    CssSelector,
    CssValueSource,
    CssAttributeName,
    DecimalSeparator,
    ThousandsSeparator,
    ExpectedCurrencyCode,
    MinimumValue,
    MaximumValue,
    TestUrl
)
VALUES
    (1, '034c567a-76ae-4d9c-bb9b-a4becb5053ec', N'Апогей 99', N'АПОГЕЙ 99', N'https://apogee99.com', N'apogee99.com', N'apogee99.com', N'www.apogee99.com', N'J1', N'Primary product JSON-LD price', N'JsonLd', N'Product', N'offers.price', N'offers.priceCurrency', NULL, NULL, NULL, N'Dot', N'None', N'EUR', 0.01, 100000.00, N'https://apogee99.com/freon-r32-0.650-kg/p/19309'),
    (2, 'f3b11722-2748-4d3b-a9b0-d3bfb42429c6', N'HomeMax', N'HOMEMAX', N'https://www.home-max.bg', N'home-max.bg', N'home-max.bg', N'www.home-max.bg', N'J1', N'Primary product JSON-LD price', N'JsonLd', N'Product', N'offers.price', N'offers.priceCurrency', NULL, NULL, NULL, N'Dot', N'None', N'EUR', 0.01, 100000.00, N'https://www.home-max.bg/blokcheta-gazobeton-100x600x250-mm/'),
    (3, 'aa1a8433-eeae-44e6-bb14-dbe8054366e7', N'BAUHAUS България', N'BAUHAUS БЪЛГАРИЯ', N'https://bauhaus.bg', N'bauhaus.bg', N'bauhaus.bg', N'www.bauhaus.bg', N'J1', N'Primary product JSON-LD price', N'JsonLd', N'Product', N'offers.price', N'offers.priceCurrency', NULL, NULL, NULL, N'Dot', N'None', N'EUR', 0.01, 100000.00, N'https://bauhaus.bg/ciment-tip-bravo-cem-iib-m-p-ll-425-n/p/9972'),
    (4, 'f62ad111-dfb8-42eb-825f-ffbd6c14f047', N'Farben', N'FARBEN', N'https://www.farben.bg', N'farben.bg', N'farben.bg', N'www.farben.bg', N'C_FARBEN', N'Visible VAT-inclusive product price', N'CssSelector', NULL, NULL, NULL, N'.product-price-container .price-wrapper > .woocommerce-Price-amount:first-child', N'TextContent', NULL, N'Dot', N'None', N'EUR', 0.01, 100000.00, N'https://www.farben.bg/gumena-malamashka-za-fugirane/'),
    (5, 'a50fa825-634e-4a0c-a1f0-c1b1c4353ba0', N'Ел Отопление', N'ЕЛ ОТОПЛЕНИЕ', N'https://el-otoplenie.com', N'el-otoplenie.com', N'el-otoplenie.com', N'www.el-otoplenie.com', N'J1', N'Primary product JSON-LD price', N'JsonLd', N'Product', N'offers.price', N'offers.priceCurrency', NULL, NULL, NULL, N'Dot', N'None', N'EUR', 0.01, 100000.00, N'https://el-otoplenie.com/podovo-otoplenie-tsena'),
    (6, '569459b2-4a65-43ea-ab79-2f3f6176bf12', N'e-Maistor', N'E-MAISTOR', N'https://e-maistor.com', N'e-maistor.com', N'e-maistor.com', N'www.e-maistor.com', N'J1', N'Primary product JSON-LD price', N'JsonLd', N'Product', N'offers.price', N'offers.priceCurrency', NULL, NULL, NULL, N'Dot', N'None', N'EUR', 0.01, 100000.00, N'https://e-maistor.com/produkt/epomax-l10-1kg/'),
    (7, 'c1241830-7377-4d32-bbd9-8f8ad77a74f8', N'Maxxmart', N'MAXXMART', N'https://www.maxxmart.eu', N'maxxmart.eu', N'maxxmart.eu', N'www.maxxmart.eu', N'J1', N'Primary product JSON-LD price', N'JsonLd', N'Product', N'offers.price', N'offers.priceCurrency', NULL, NULL, NULL, N'Dot', N'None', N'EUR', 0.01, 100000.00, N'https://www.maxxmart.eu/product/siv-tsiment-cem-ii-325-r-holcim'),
    (8, '376b37b7-21a0-43b8-a5ac-932b0cd42e6e', N'Praktis', N'PRAKTIS', N'https://praktis.bg', N'praktis.bg', N'praktis.bg', N'www.praktis.bg', N'J1', N'Primary product JSON-LD price', N'JsonLd', N'Product', N'offers.price', N'offers.priceCurrency', NULL, NULL, NULL, N'Dot', N'None', N'EUR', 0.01, 100000.00, N'https://praktis.bg/tsiment-holcim-cem-ii-c-m-v-ll-42-5-n-25-kg'),
    (9, '8f7e9641-5559-46e6-a85f-5141daf0e51a', N'MIP Trade', N'MIP TRADE', N'https://miptrade.com', N'miptrade.com', N'miptrade.com', N'www.miptrade.com', N'J1', N'Primary product JSON-LD price', N'JsonLd', N'Product', N'offers.price', N'offers.priceCurrency', NULL, NULL, NULL, N'Dot', N'None', N'EUR', 0.01, 100000.00, N'https://miptrade.com/pro-tec-xxl-diagonal-s-poluavtomatichno-zakrepvane-912879-krause-7569'),
    (10, 'd8479494-ad9b-4ed0-b477-b46520e59b52', N'Поларис Продукт', N'ПОЛАРИС ПРОДУКТ', N'https://polaris-bg.com', N'polaris-bg.com', N'polaris-bg.com', N'www.polaris-bg.com', N'C_POLARIS', N'Visible VAT-inclusive product price', N'CssSelector', NULL, NULL, NULL, N'.product-info .price-wrap > p.price', N'TextContent', NULL, N'Comma', N'None', N'EUR', 0.01, 100000.00, N'https://polaris-bg.com/bg/gipskarton-knauf-a13-1200-2500-mm/p155.html'),
    (11, '6301c610-6512-454e-ae5d-786ca84f1ce7', N'Praktiker', N'PRAKTIKER', N'https://praktiker.bg', N'praktiker.bg', N'praktiker.bg', N'www.praktiker.bg', N'J1', N'Primary product JSON-LD price', N'JsonLd', N'Product', N'offers.price', N'offers.priceCurrency', NULL, NULL, NULL, N'Dot', N'None', N'EUR', 0.01, 100000.00, N'https://praktiker.bg/bg/Stroitelni-materiali/Tsiment-HOLCIM-CEM-II-C-M-V-LL-42-5-N-25-kg/p/129576'),
    (12, '0f4400ab-de38-4a2c-a683-09ce82363df5', N'LED Zona', N'LED ZONA', N'https://www.led-zona.bg', N'led-zona.bg', N'led-zona.bg', N'www.led-zona.bg', N'J1', N'Primary product JSON-LD price', N'JsonLd', N'Product', N'offers.price', N'offers.priceCurrency', NULL, NULL, NULL, N'Dot', N'None', N'EUR', 0.01, 100000.00, N'https://www.led-zona.bg/p/led-lenta-ip65-24vdc-4-8w-m-520lm-m-6500k-smd2835-60-leds-m-rolka-5-metra-led-labs-16-1066-52/'),
    (13, '9afdc138-e7fd-4dbb-a345-a479eb68ad6a', N'Стоичкови', N'СТОИЧКОВИ', N'https://stoichkovi.bg', N'stoichkovi.bg', N'stoichkovi.bg', N'www.stoichkovi.bg', N'C_STOICHKOVI', N'Visible product price', N'CssSelector', NULL, NULL, NULL, N'.main .product-price-holder span', N'TextContent', NULL, N'Dot', N'None', N'EUR', 0.01, 100000.00, N'https://stoichkovi.bg/spirt-evrohim-0-5l-pochistvane-gorene'),
    (14, '4ce57854-b2d8-4b95-b7db-52b0ed571ad2', N'Ташев-Галвинг', N'ТАШЕВ-ГАЛВИНГ', N'https://tashev-galving.com', N'tashev-galving.com', N'tashev-galving.com', N'www.tashev-galving.com', N'J1', N'Primary product JSON-LD price', N'JsonLd', N'Product', N'offers.price', N'offers.priceCurrency', NULL, NULL, NULL, N'Dot', N'None', N'EUR', 0.01, 100000.00, N'https://tashev-galving.com/product/141-MKT-467/elektricheski-agloshlayf-makita-720-w-125-mm-11-000-ob-min-ga5030r'),
    (15, '60f4cf9a-0afe-4db4-b4e0-ae124ac85199', N'ADAX България', N'ADAX БЪЛГАРИЯ', N'https://www.adax.bg', N'adax.bg', N'adax.bg', N'www.adax.bg', N'C_ADAX', N'Visible EUR product price', N'CssSelector', NULL, NULL, NULL, N'.shopping .itm-price-show.price-in-EUR', N'TextContent', NULL, N'Dot', N'None', N'EUR', 0.01, 100000.00, N'https://www.adax.bg/bg/article-8-konvektor-adax-neo-basic-np-04-kdt.htm'),
    (16, 'c64f0130-933a-4535-bd3a-9f7f33f0414c', N'Technopolis', N'TECHNOPOLIS', N'https://www.technopolis.bg', N'technopolis.bg', N'technopolis.bg', N'www.technopolis.bg', N'J1', N'Primary product JSON-LD price', N'JsonLd', N'Product', N'offers.price', N'offers.priceCurrency', NULL, NULL, NULL, N'Dot', N'None', N'EUR', 0.01, 100000.00, N'https://www.technopolis.bg/bg/Televizori/Televizor-SMARTTECH-24HW01V3/p/14851'),
    (17, '29f0246e-c469-4a11-90dd-1d21a60b7c41', N'TMT Elkom', N'TMT ELKOM', N'https://www.tmt-elkom.com', N'tmt-elkom.com', N'tmt-elkom.com', N'www.tmt-elkom.com', N'C_TMT', N'Visible VAT-inclusive product price', N'CssSelector', NULL, NULL, NULL, N'.prices .product-price > span', N'TextContent', NULL, N'Comma', N'None', N'EUR', 0.01, 100000.00, N'https://www.tmt-elkom.com/prekasvach-easy9-1p-16a-ez9f35116'),
    (18, '51f4e65f-271f-45e4-b3fa-c14e540bdd3a', N'Топ Груп Плюс', N'ТОП ГРУП ПЛЮС', N'https://tgp.bg', N'tgp.bg', N'tgp.bg', N'www.tgp.bg', N'C_TGP', N'Product data-price', N'CssSelector', NULL, NULL, NULL, N'#product-main-summary', N'Attribute', N'data-price', N'Dot', N'None', N'EUR', 0.01, 100000.00, N'https://tgp.bg/produkt/p17-prenosim-shtepsel-16a-380v-3pe/'),
    (19, '76ba2290-1d14-4028-8aa8-dda8574fbcaf', N'JYSK България', N'JYSK БЪЛГАРИЯ', N'https://jysk.bg', N'jysk.bg', N'jysk.bg', N'www.jysk.bg', N'J1', N'Primary product JSON-LD price', N'JsonLd', N'Product', N'offers.price', N'offers.priceCurrency', NULL, NULL, NULL, N'Dot', N'None', N'EUR', 0.01, 100000.00, N'https://jysk.bg/spalnya/zavivki/zavivki-s-pukh/zavivka-s-pateshki-pukh-135x200-galdeberget-topleshcha');

IF (SELECT COUNT_BIG(*) FROM @Import) <> 19
    THROW 51001, N'The import must contain exactly 19 retailer definitions.', 1;

IF EXISTS
(
    SELECT PersonId
    FROM @Import
    GROUP BY PersonId
    HAVING COUNT_BIG(*) > 1
)
    THROW 51002, N'The import definitions contain duplicate PersonId values.', 1;

IF EXISTS
(
    SELECT NormalizedName
    FROM @Import
    GROUP BY NormalizedName
    HAVING COUNT_BIG(*) > 1
)
    THROW 51003, N'The import definitions contain duplicate NormalizedName values.', 1;

IF EXISTS
(
    SELECT 1
    FROM @Import
    WHERE DisplayName = N'House of Sport'
       OR NormalizedName = N'HOUSE OF SPORT'
)
    THROW 51004, N'House of Sport must not be included in this EUR-only import.', 1;

IF EXISTS
(
    SELECT 1
    FROM @Import
    WHERE LEN(DisplayName) = 0
       OR LEN(DisplayName) > 200
       OR LEN(NormalizedName) = 0
       OR LEN(NormalizedName) > 400
       OR LEN(BaseWebsiteUrl) = 0
       OR LEN(BaseWebsiteUrl) > 2048
       OR RIGHT(BaseWebsiteUrl, 1) = N'/'
       OR NormalizedWebsiteHost <> AllowedHost1
       OR AllowedHost2 <> N'www.' + NormalizedWebsiteHost
       OR AllowedHost1 = AllowedHost2
       OR LEN(AllowedHost1) > 253
       OR LEN(AllowedHost2) > 253
       OR ExpectedCurrencyCode <> N'EUR'
       OR DecimalSeparator NOT IN (N'Dot', N'Comma')
       OR ThousandsSeparator <> N'None'
       OR MinimumValue <> 0.01
       OR MaximumValue <> 100000.00
       OR LEN(TestUrl) = 0
       OR LEN(TestUrl) > 2048
)
    THROW 51005, N'One or more import definitions violate the expected normalized values or limits.', 1;

IF EXISTS
(
    SELECT 1
    FROM @Import
    WHERE NOT
    (
        (
            RuleType = N'JsonLd'
            AND JsonLdObjectType = N'Product'
            AND JsonLdPricePath = N'offers.price'
            AND JsonLdCurrencyPath = N'offers.priceCurrency'
            AND CssSelector IS NULL
            AND CssValueSource IS NULL
            AND CssAttributeName IS NULL
        )
        OR
        (
            RuleType = N'CssSelector'
            AND JsonLdObjectType IS NULL
            AND JsonLdPricePath IS NULL
            AND JsonLdCurrencyPath IS NULL
            AND CssSelector IS NOT NULL
            AND
            (
                (CssValueSource = N'TextContent' AND CssAttributeName IS NULL)
                OR
                (CssValueSource = N'Attribute' AND CssAttributeName IS NOT NULL)
            )
        )
    )
)
    THROW 51006, N'One or more extraction-rule definitions have an invalid persisted shape.', 1;

DECLARE @Plan TABLE
(
    ImportOrder int NOT NULL PRIMARY KEY,
    PersonId uniqueidentifier NOT NULL,
    RetailerExists bit NOT NULL,
    ProfileExists bit NOT NULL,
    NewRetailerId uniqueidentifier NULL,
    NewProfileId uniqueidentifier NULL,
    NewRuleId uniqueidentifier NULL
);

DECLARE @ActionReport TABLE
(
    ImportOrder int NOT NULL,
    EntityOrder int NOT NULL,
    CompanyPersonId uniqueidentifier NOT NULL,
    RetailerName nvarchar(200) NOT NULL,
    EntityType nvarchar(16) NOT NULL,
    ActionStatus nvarchar(64) NOT NULL,
    Explanation nvarchar(1000) NOT NULL,
    TestUrl nvarchar(2048) NOT NULL
);

DECLARE @RetailerVerification TABLE
(
    ImportOrder int NOT NULL,
    CompanyPersonId uniqueidentifier NOT NULL,
    ImportRetailerName nvarchar(200) NOT NULL,
    RetailerId uniqueidentifier NOT NULL,
    PersistedDisplayName nvarchar(200) NOT NULL,
    BaseWebsiteUrl nvarchar(2048) NOT NULL,
    NormalizedWebsiteHost nvarchar(253) NOT NULL,
    IsActive bit NOT NULL,
    IsDefaultForOffers bit NOT NULL
);

DECLARE @ProfileVerification TABLE
(
    ImportOrder int NOT NULL,
    CompanyPersonId uniqueidentifier NOT NULL,
    RetailerName nvarchar(200) NOT NULL,
    ProfileId uniqueidentifier NOT NULL,
    Version int NOT NULL,
    Status nvarchar(16) NOT NULL,
    IsActive bit NOT NULL,
    ConfigurationRevision bigint NOT NULL,
    ValidatedConfigurationRevision bigint NULL,
    PublishedAt datetimeoffset NULL,
    PublishedBy nvarchar(450) NULL,
    LastSuccessfulTestAt datetimeoffset NULL,
    LastSuccessfulTestRuleId uniqueidentifier NULL
);

DECLARE @HostVerification TABLE
(
    ImportOrder int NOT NULL,
    CompanyPersonId uniqueidentifier NOT NULL,
    RetailerName nvarchar(200) NOT NULL,
    ProfileId uniqueidentifier NOT NULL,
    AllowedHostId uniqueidentifier NOT NULL,
    NormalizedHost nvarchar(253) NOT NULL
);

DECLARE @RuleVerification TABLE
(
    ImportOrder int NOT NULL,
    CompanyPersonId uniqueidentifier NOT NULL,
    RetailerName nvarchar(200) NOT NULL,
    ProfileId uniqueidentifier NOT NULL,
    RuleId uniqueidentifier NOT NULL,
    RuleCode nvarchar(32) NOT NULL,
    RuleName nvarchar(200) NOT NULL,
    Priority int NOT NULL,
    IsEnabled bit NOT NULL,
    RuleType nvarchar(24) NOT NULL,
    DecimalSeparator nvarchar(16) NOT NULL,
    ThousandsSeparator nvarchar(16) NOT NULL,
    ExpectedCurrencyCode nvarchar(3) NOT NULL,
    TestUrl nvarchar(2048) NOT NULL
);

SET TRANSACTION ISOLATION LEVEL SERIALIZABLE;

BEGIN TRY
    BEGIN TRANSACTION;

    IF DB_NAME() <> N'SiteWatch'
        THROW 51007, N'This import may run only against the SiteWatch database.', 1;

    IF OBJECT_ID(N'dbo.__EFMigrationsHistory', N'U') IS NULL
        THROW 51008, N'The EF migration history table is missing.', 1;

    IF NOT EXISTS
    (
        SELECT 1
        FROM dbo.__EFMigrationsHistory WITH (HOLDLOCK)
        WHERE MigrationId = N'20261114100024_MovePriceBasisToRetailerListing'
    )
        THROW 51009, N'Required migration 20261114100024_MovePriceBasisToRetailerListing is not applied.', 1;

    IF OBJECT_ID(N'dbo.RetailerExtractionRules', N'U') IS NULL
       OR OBJECT_ID(N'dbo.RetailerListings', N'U') IS NULL
       OR COL_LENGTH(N'dbo.RetailerExtractionRules', N'PriceBasis') IS NOT NULL
       OR COL_LENGTH(N'dbo.RetailerListings', N'OnlinePriceBasis') IS NULL
        THROW 51010, N'The extraction schema does not match MovePriceBasisToRetailerListing.', 1;

    IF EXISTS
    (
        SELECT 1
        FROM @Import AS import
        LEFT JOIN dbo.Persons AS person WITH (UPDLOCK, HOLDLOCK)
            ON person.Id = import.PersonId
        WHERE person.Id IS NULL
           OR person.Type <> 1
    )
    BEGIN
        SELECT
            import.PersonId,
            import.DisplayName AS RetailerName,
            CASE
                WHEN person.Id IS NULL THEN N'MISSING_PERSON'
                ELSE N'PERSON_IS_NOT_COMPANY'
            END AS ValidationFailure
        FROM @Import AS import
        LEFT JOIN dbo.Persons AS person WITH (UPDLOCK, HOLDLOCK)
            ON person.Id = import.PersonId
        WHERE person.Id IS NULL
           OR person.Type <> 1
        ORDER BY import.ImportOrder;

        THROW 51011, N'Every imported PersonId must reference an existing company Person.', 1;
    END;

    INSERT INTO @Plan
    (
        ImportOrder,
        PersonId,
        RetailerExists,
        ProfileExists,
        NewRetailerId,
        NewProfileId,
        NewRuleId
    )
    SELECT
        import.ImportOrder,
        import.PersonId,
        CONVERT(bit, CASE WHEN retailerState.ExistingCount > 0 THEN 1 ELSE 0 END),
        CONVERT(bit, CASE WHEN profileState.ExistingCount > 0 THEN 1 ELSE 0 END),
        CASE WHEN retailerState.ExistingCount = 0 THEN NEWID() END,
        CASE WHEN profileState.ExistingCount = 0 THEN NEWID() END,
        CASE WHEN profileState.ExistingCount = 0 THEN NEWID() END
    FROM @Import AS import
    CROSS APPLY
    (
        SELECT COUNT_BIG(*) AS ExistingCount
        FROM dbo.Retailers WITH (UPDLOCK, HOLDLOCK)
        WHERE CompanyPersonId = import.PersonId
    ) AS retailerState
    CROSS APPLY
    (
        SELECT COUNT_BIG(*) AS ExistingCount
        FROM dbo.RetailerExtractionProfiles WITH (UPDLOCK, HOLDLOCK)
        WHERE CompanyPersonId = import.PersonId
    ) AS profileState;

    IF EXISTS
    (
        SELECT 1
        FROM @Import AS import
        INNER JOIN @Plan AS importPlan
            ON importPlan.PersonId = import.PersonId
        INNER JOIN dbo.Retailers AS existingRetailer WITH (UPDLOCK, HOLDLOCK)
            ON existingRetailer.NormalizedName = import.NormalizedName
           AND existingRetailer.CompanyPersonId <> import.PersonId
        WHERE importPlan.RetailerExists = 0
    )
    BEGIN
        SELECT
            import.PersonId,
            import.DisplayName AS RequestedRetailerName,
            existingRetailer.CompanyPersonId AS ConflictingCompanyPersonId,
            existingRetailer.DisplayName AS ConflictingRetailerName
        FROM @Import AS import
        INNER JOIN @Plan AS importPlan
            ON importPlan.PersonId = import.PersonId
        INNER JOIN dbo.Retailers AS existingRetailer WITH (UPDLOCK, HOLDLOCK)
            ON existingRetailer.NormalizedName = import.NormalizedName
           AND existingRetailer.CompanyPersonId <> import.PersonId
        WHERE importPlan.RetailerExists = 0
        ORDER BY import.ImportOrder;

        THROW 51012, N'A requested normalized retailer name is already owned by another company Person.', 1;
    END;

    INSERT INTO dbo.Retailers
    (
        Id,
        DisplayName,
        NormalizedName,
        CompanyPersonId,
        BaseWebsiteUrl,
        NormalizedWebsiteHost,
        Notes,
        IsActive,
        IsDefaultForOffers,
        Created,
        CreatedBy,
        LastModified,
        LastModifiedBy
    )
    SELECT
        importPlan.NewRetailerId,
        import.DisplayName,
        import.NormalizedName,
        import.PersonId,
        import.BaseWebsiteUrl,
        import.NormalizedWebsiteHost,
        NULL,
        1,
        0,
        @ImportedAt,
        @AuditActor,
        @ImportedAt,
        @AuditActor
    FROM @Import AS import
    INNER JOIN @Plan AS importPlan
        ON importPlan.PersonId = import.PersonId
    WHERE importPlan.RetailerExists = 0;

    INSERT INTO dbo.RetailerExtractionProfiles
    (
        Id,
        CompanyPersonId,
        Version,
        Status,
        IsActive,
        PublishedAt,
        PublishedBy,
        ConfigurationRevision,
        ValidatedConfigurationRevision,
        LastSuccessfulTestAt,
        LastSuccessfulTestRuleId,
        Created,
        CreatedBy,
        LastModified,
        LastModifiedBy
    )
    SELECT
        importPlan.NewProfileId,
        import.PersonId,
        1,
        N'Draft',
        0,
        NULL,
        NULL,
        2,
        NULL,
        NULL,
        NULL,
        @ImportedAt,
        @AuditActor,
        @ImportedAt,
        @AuditActor
    FROM @Import AS import
    INNER JOIN @Plan AS importPlan
        ON importPlan.PersonId = import.PersonId
    WHERE importPlan.ProfileExists = 0;

    INSERT INTO dbo.RetailerExtractionAllowedHosts
    (
        Id,
        ExtractionProfileId,
        NormalizedHost
    )
    SELECT
        NEWID(),
        importPlan.NewProfileId,
        allowedHost.NormalizedHost
    FROM @Import AS import
    INNER JOIN @Plan AS importPlan
        ON importPlan.PersonId = import.PersonId
    CROSS APPLY
    (
        VALUES
            (import.AllowedHost1),
            (import.AllowedHost2)
    ) AS allowedHost(NormalizedHost)
    WHERE importPlan.ProfileExists = 0;

    INSERT INTO dbo.RetailerExtractionRules
    (
        Id,
        ExtractionProfileId,
        Name,
        IsEnabled,
        Priority,
        RuleType,
        JsonLdObjectType,
        JsonLdPricePath,
        JsonLdCurrencyPath,
        CssSelector,
        CssValueSource,
        CssAttributeName,
        DecimalSeparator,
        ThousandsSeparator,
        ExpectedCurrencyCode,
        MinimumValue,
        MaximumValue
    )
    SELECT
        importPlan.NewRuleId,
        importPlan.NewProfileId,
        import.RuleName,
        1,
        1,
        import.RuleType,
        import.JsonLdObjectType,
        import.JsonLdPricePath,
        import.JsonLdCurrencyPath,
        import.CssSelector,
        import.CssValueSource,
        import.CssAttributeName,
        import.DecimalSeparator,
        import.ThousandsSeparator,
        import.ExpectedCurrencyCode,
        import.MinimumValue,
        import.MaximumValue
    FROM @Import AS import
    INNER JOIN @Plan AS importPlan
        ON importPlan.PersonId = import.PersonId
    WHERE importPlan.ProfileExists = 0;

    IF EXISTS
    (
        SELECT 1
        FROM @Plan AS importPlan
        INNER JOIN @Import AS import
            ON import.PersonId = importPlan.PersonId
        WHERE importPlan.RetailerExists = 0
          AND NOT EXISTS
          (
              SELECT 1
              FROM dbo.Retailers AS retailer
              WHERE retailer.Id = importPlan.NewRetailerId
                AND retailer.CompanyPersonId = importPlan.PersonId
                AND retailer.DisplayName = import.DisplayName
                AND retailer.NormalizedName = import.NormalizedName
                AND retailer.BaseWebsiteUrl = import.BaseWebsiteUrl
                AND retailer.NormalizedWebsiteHost = import.NormalizedWebsiteHost
                AND retailer.IsActive = 1
                AND retailer.IsDefaultForOffers = 0
          )
    )
        THROW 51013, N'A newly inserted retailer failed post-write verification.', 1;

    IF EXISTS
    (
        SELECT 1
        FROM @Plan AS importPlan
        WHERE importPlan.ProfileExists = 0
          AND
          (
              NOT EXISTS
              (
                  SELECT 1
                  FROM dbo.RetailerExtractionProfiles AS profile
                  WHERE profile.Id = importPlan.NewProfileId
                    AND profile.CompanyPersonId = importPlan.PersonId
                    AND profile.Version = 1
                    AND profile.Status = N'Draft'
                    AND profile.IsActive = 0
                    AND profile.ConfigurationRevision = 2
                    AND profile.PublishedAt IS NULL
                    AND profile.PublishedBy IS NULL
                    AND profile.ValidatedConfigurationRevision IS NULL
                    AND profile.LastSuccessfulTestAt IS NULL
                    AND profile.LastSuccessfulTestRuleId IS NULL
              )
              OR
              (
                  SELECT COUNT_BIG(*)
                  FROM dbo.RetailerExtractionAllowedHosts AS allowedHost
                  WHERE allowedHost.ExtractionProfileId = importPlan.NewProfileId
              ) <> 2
              OR
              (
                  SELECT COUNT_BIG(*)
                  FROM dbo.RetailerExtractionRules AS extractionRule
                  WHERE extractionRule.ExtractionProfileId = importPlan.NewProfileId
                    AND extractionRule.IsEnabled = 1
                    AND extractionRule.Priority = 1
              ) <> 1
          )
    )
        THROW 51014, N'A newly inserted draft failed post-write verification.', 1;

    INSERT INTO @ActionReport
    (
        ImportOrder,
        EntityOrder,
        CompanyPersonId,
        RetailerName,
        EntityType,
        ActionStatus,
        Explanation,
        TestUrl
    )
    SELECT
        import.ImportOrder,
        1,
        import.PersonId,
        import.DisplayName,
        N'RETAILER',
        CASE
            WHEN importPlan.RetailerExists = 1 THEN N'SKIPPED_EXISTING_RETAILER'
            WHEN @Apply = 0 THEN N'WOULD_CREATE_RETAILER'
            ELSE N'CREATED_RETAILER'
        END,
        CASE
            WHEN importPlan.RetailerExists = 1
                THEN N'At least one retailer already exists for this company Person; all existing retailer data was preserved.'
            WHEN @Apply = 0
                THEN N'Would create one active retailer with IsDefaultForOffers = 0.'
            ELSE N'Created one active retailer with IsDefaultForOffers = 0.'
        END,
        import.TestUrl
    FROM @Import AS import
    INNER JOIN @Plan AS importPlan
        ON importPlan.PersonId = import.PersonId

    UNION ALL

    SELECT
        import.ImportOrder,
        2,
        import.PersonId,
        import.DisplayName,
        N'PROFILE',
        CASE
            WHEN importPlan.ProfileExists = 1 THEN N'SKIPPED_EXISTING_PROFILE'
            WHEN @Apply = 0 THEN N'WOULD_CREATE_DRAFT'
            ELSE N'CREATED_DRAFT'
        END,
        CASE
            WHEN importPlan.ProfileExists = 1
                THEN N'At least one extraction-profile version already exists; all existing profiles, hosts, and rules were preserved.'
            WHEN @Apply = 0
                THEN CONCAT(N'Would create draft version 1 with two allowed hosts, enabled rule ', import.RuleCode, N', and ConfigurationRevision = 2.')
            ELSE CONCAT(N'Created draft version 1 with two allowed hosts, enabled rule ', import.RuleCode, N', and ConfigurationRevision = 2.')
        END,
        import.TestUrl
    FROM @Import AS import
    INNER JOIN @Plan AS importPlan
        ON importPlan.PersonId = import.PersonId;

    IF (SELECT COUNT_BIG(*) FROM @ActionReport) <> 38
        THROW 51015, N'The action report must contain exactly two rows for each import definition.', 1;

    INSERT INTO @RetailerVerification
    SELECT
        import.ImportOrder,
        import.PersonId,
        import.DisplayName,
        retailer.Id,
        retailer.DisplayName,
        retailer.BaseWebsiteUrl,
        retailer.NormalizedWebsiteHost,
        retailer.IsActive,
        retailer.IsDefaultForOffers
    FROM @Import AS import
    INNER JOIN dbo.Retailers AS retailer
        ON retailer.CompanyPersonId = import.PersonId;

    INSERT INTO @ProfileVerification
    SELECT
        import.ImportOrder,
        import.PersonId,
        import.DisplayName,
        profile.Id,
        profile.Version,
        profile.Status,
        profile.IsActive,
        profile.ConfigurationRevision,
        profile.ValidatedConfigurationRevision,
        profile.PublishedAt,
        profile.PublishedBy,
        profile.LastSuccessfulTestAt,
        profile.LastSuccessfulTestRuleId
    FROM @Import AS import
    INNER JOIN dbo.RetailerExtractionProfiles AS profile
        ON profile.CompanyPersonId = import.PersonId;

    INSERT INTO @HostVerification
    SELECT
        import.ImportOrder,
        import.PersonId,
        import.DisplayName,
        profile.Id,
        allowedHost.Id,
        allowedHost.NormalizedHost
    FROM @Import AS import
    INNER JOIN dbo.RetailerExtractionProfiles AS profile
        ON profile.CompanyPersonId = import.PersonId
    INNER JOIN dbo.RetailerExtractionAllowedHosts AS allowedHost
        ON allowedHost.ExtractionProfileId = profile.Id;

    INSERT INTO @RuleVerification
    SELECT
        import.ImportOrder,
        import.PersonId,
        import.DisplayName,
        profile.Id,
        extractionRule.Id,
        import.RuleCode,
        extractionRule.Name,
        extractionRule.Priority,
        extractionRule.IsEnabled,
        extractionRule.RuleType,
        extractionRule.DecimalSeparator,
        extractionRule.ThousandsSeparator,
        extractionRule.ExpectedCurrencyCode,
        import.TestUrl
    FROM @Import AS import
    INNER JOIN dbo.RetailerExtractionProfiles AS profile
        ON profile.CompanyPersonId = import.PersonId
    INNER JOIN dbo.RetailerExtractionRules AS extractionRule
        ON extractionRule.ExtractionProfileId = profile.Id;

    IF @Apply = 0
    BEGIN
        SELECT
            N'PREVIEW_SIMULATED_RETAILERS' AS VerificationSet,
            CompanyPersonId,
            ImportRetailerName,
            RetailerId,
            PersistedDisplayName,
            BaseWebsiteUrl,
            NormalizedWebsiteHost,
            IsActive,
            IsDefaultForOffers
        FROM @RetailerVerification
        ORDER BY ImportOrder, PersistedDisplayName, RetailerId;

        SELECT
            N'PREVIEW_SIMULATED_PROFILES' AS VerificationSet,
            CompanyPersonId,
            RetailerName,
            ProfileId,
            Version,
            Status,
            IsActive,
            ConfigurationRevision,
            ValidatedConfigurationRevision,
            PublishedAt,
            PublishedBy,
            LastSuccessfulTestAt,
            LastSuccessfulTestRuleId
        FROM @ProfileVerification
        ORDER BY ImportOrder, Version, ProfileId;

        SELECT
            N'PREVIEW_SIMULATED_HOSTS' AS VerificationSet,
            CompanyPersonId,
            RetailerName,
            ProfileId,
            AllowedHostId,
            NormalizedHost
        FROM @HostVerification
        ORDER BY ImportOrder, ProfileId, NormalizedHost;

        SELECT
            N'PREVIEW_SIMULATED_RULES' AS VerificationSet,
            CompanyPersonId,
            RetailerName,
            ProfileId,
            RuleId,
            RuleCode,
            RuleName,
            Priority,
            IsEnabled,
            RuleType,
            DecimalSeparator,
            ThousandsSeparator,
            ExpectedCurrencyCode,
            TestUrl
        FROM @RuleVerification
        ORDER BY ImportOrder, ProfileId, Priority, RuleId;

        ROLLBACK TRANSACTION;
    END
    ELSE
    BEGIN
        COMMIT TRANSACTION;

        SELECT
            N'APPLIED_RETAILERS' AS VerificationSet,
            CompanyPersonId,
            ImportRetailerName,
            RetailerId,
            PersistedDisplayName,
            BaseWebsiteUrl,
            NormalizedWebsiteHost,
            IsActive,
            IsDefaultForOffers
        FROM @RetailerVerification
        ORDER BY ImportOrder, PersistedDisplayName, RetailerId;

        SELECT
            N'APPLIED_PROFILES' AS VerificationSet,
            CompanyPersonId,
            RetailerName,
            ProfileId,
            Version,
            Status,
            IsActive,
            ConfigurationRevision,
            ValidatedConfigurationRevision,
            PublishedAt,
            PublishedBy,
            LastSuccessfulTestAt,
            LastSuccessfulTestRuleId
        FROM @ProfileVerification
        ORDER BY ImportOrder, Version, ProfileId;

        SELECT
            N'APPLIED_HOSTS' AS VerificationSet,
            CompanyPersonId,
            RetailerName,
            ProfileId,
            AllowedHostId,
            NormalizedHost
        FROM @HostVerification
        ORDER BY ImportOrder, ProfileId, NormalizedHost;

        SELECT
            N'APPLIED_RULES' AS VerificationSet,
            CompanyPersonId,
            RetailerName,
            ProfileId,
            RuleId,
            RuleCode,
            RuleName,
            Priority,
            IsEnabled,
            RuleType,
            DecimalSeparator,
            ThousandsSeparator,
            ExpectedCurrencyCode,
            TestUrl
        FROM @RuleVerification
        ORDER BY ImportOrder, ProfileId, Priority, RuleId;
    END;

    SELECT
        CASE WHEN @Apply = 0 THEN N'PREVIEW_ROLLED_BACK' ELSE N'APPLIED_AND_COMMITTED' END AS ExecutionMode,
        CompanyPersonId,
        RetailerName,
        EntityType,
        ActionStatus,
        Explanation,
        TestUrl
    FROM @ActionReport
    ORDER BY ImportOrder, EntityOrder;
END TRY
BEGIN CATCH
    IF XACT_STATE() <> 0
        ROLLBACK TRANSACTION;

    THROW;
END CATCH;
