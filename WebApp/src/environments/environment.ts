export const environment = {
        production: false,
        local: false,
        HOST: {
                WEB_URL: "test.api-efms.itlvn.com",
                ACCOUNTING: "test.api-efms.itlvn.com/Accounting",
                // ACCOUNTING: "localhost:44368",
                DOCUMENTATION: "test.api-efms.itlvn.com/Documentation",
                SYSTEM: "test.api-efms.itlvn.com/System",
                //SYSTEM: "localhost:44360",
                SETTING: "test.api-efms.itlvn.com/Setting",
                REPORT: "http://test.api-efms.itlvn.com/ReportPreview/Default.aspx",
                // REPORT: "http://localhost:53717",
                EXPORT: "test.api-efms.itlvn.com/Export",
                // EXPORT: "localhost:63492",
                INDENTITY_SERVER_URL: "http://test.api-efms.itlvn.com/identityserver",
                // INDENTITY_SERVER_URL: "https://localhost:44369",
                // EXPORT_CRYSTAL: "localhost:53717/ExportCrystal.aspx",
                EXPORT_CRYSTAL: "http://test.api-efms.itlvn.com/ReportPreview/ExportCrystal.aspx",
                PARTNER_API: "test.api-efms.itlvn.com/partner"
        },

        AUTHORIZATION:
        {
                requireHttps: false
        },
};

