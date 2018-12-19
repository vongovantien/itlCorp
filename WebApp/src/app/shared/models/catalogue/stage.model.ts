// import { moment } from "ngx-bootstrap/chronos/test/chain";

export class StageModel {
    constructor(){}
    public id: Number;
    public code: String;
    public stageNameVn: String;
    public stageNameEn: String;
    public departmentId: Number;
    public descriptionVn: String;
    public descriptionEn: String;
    public userCreated: String;
    public datetimeCreated: Date;
    public userModified: String;
    public datetimeModified: Date;
    public inactive?: Boolean;
    public inactiveOn?: Date;
}