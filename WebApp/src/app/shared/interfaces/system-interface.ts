namespace SystemInterface {
    export interface IClaimUser {
        email: string;
        employeeId: string;
        id: string;
        phone_number: string;
        preferred_username: string;
        sub: string;
        userName: string;
        companyId: string;
        officeId: string;
        departmentId: number;
        groupId: number;
    }

    export interface IDepartmentGroup {
        userId: string;
        departmentId: number;
        groupId: number;
        departmentName: string;
        groupName: string;
    }
}
