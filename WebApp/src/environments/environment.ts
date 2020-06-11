
export const environment = {
        production: false,
        local: false,
        HOST: {
                WEB_URL: "test.api-efms.itlvn.com",
                ACCOUNTING: "test.api-efms.itlvn.com/Accounting",
                // ACCOUNTING: "localhost:44368",
                DOCUMENTATION: "test.api-efms.itlvn.com/Documentation",
                // DOCUMENTATION: "localhost:44366",
                CATALOGUE: "test.api-efms.itlvn.com/Catalogue",
                // CATALOGUE: "localhost:44361",
                OPERATION: "test.api-efms.itlvn.com/Operation",
                // OPERATION: "localhost:44365",
                SYSTEM: "test.api-efms.itlvn.com/System",
                // SYSTEM: "localhost:44360",
                SETTING: "test.api-efms.itlvn.com/Setting",
                // SETTING: "localhost:44363",
                REPORT: "http://test.api-efms.itlvn.com/ReportPreview/Default.aspx",
                // REPORT: "http://localhost:53717",
                EXPORT: "test.api-efms.itlvn.com/Export",
                // EXPORT: "localhost:63492",
                INDENTITY_SERVER_URL: "http://test.api-efms.itlvn.com/identityserver",
                // INDENTITY_SERVER_URL: "https://localhost:44369",
                EXPORT_CRYSTAL: "http://localhost:53717/ExportCrystal.aspx",
                // EXPORT_CRYSTAL: "http://test.api-efms.itlvn.com/ReportPreview/ExportCrystal.aspx",
        },
        AUTHORIZATION:
        {
                requireHttps: false
        }
};
