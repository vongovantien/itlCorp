// tslint:disable-next-line: file-name-casing
export class SysUserNotification {
    id: string;
    type: string;
    title: string;
    description: string;
    action: string;
    actionLink: string;
    status: STATUS_NOTI;
    datetimeCreated: string;
    datetimeModified: string;
    userCreated: string;
    userModified: string;
    userId: string;
}

type STATUS_NOTI = 'New' | 'Read' | 'Closed'; 
