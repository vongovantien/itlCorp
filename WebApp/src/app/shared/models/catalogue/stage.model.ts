export class StageModel {
    id: Number;
    code: String;
    stageNameVn: String;
    stageNameEn: String;
    departmentId: Number;
    descriptionVn: String;
    descriptionEn: String;
    userCreated: String;
    datetimeCreated: Date;
    userModified: String;
    datetimeModified: Date;
    inactive?: Boolean;
    inactiveOn?: Date;
}