import { Injectable } from "@angular/core";
import * as moment from "moment";

@Injectable()
export class CommonFunctions {
  static dateToString(date: Date): string {
    let day = date.getDate().toString();
    let month = (date.getMonth() + 1).toString();
    let year = date.getFullYear().toString();

    if (day.length === 1) {
      day = "0" + day;
    }

    if (month.length === 1) {
      month = "0" + month;
    }

    return year + month + day;
  }

  static stringToDate(date: string): Date {
    let year = parseInt(date.substr(0, 4));
    let month = parseInt(date.substr(4, 2)) - 1;
    let day = parseInt(date.substr(6, 2));

    return new Date(year, month, day);
  }

  static toMoment(date: Date): moment.Moment {
    if (date === null) {
      throw new Error("date cannot be null");
    }

    var month = date.getMonth() + 1; //months from 1-12
    var day = date.getDate();
    var year = date.getFullYear();

    return moment({ year: year, month: month - 1, day: day });
  }
}
