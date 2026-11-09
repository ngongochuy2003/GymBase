export type NotificationChannelId =
  | 'gymbase_urgent_messages'
  | 'gymbase_alarms'
  | 'gymbase_general';

export interface UrgentMessageData {
  conversation_id?: string;
  sender_id?: string;
  sender_name?: string;
  sender_avatar?: string;
  action_url?: string;
  type: 'chat_message' | 'workout_reminder' | 'alarm';
}

export interface LockScreenNotificationOptions {
  title: string;
  body: string;
  channelId?: NotificationChannelId;
  data?: UrgentMessageData;
  sound?: boolean | string;
  vibrate?: boolean;
}

export interface AlarmScheduleOptions {
  title: string;
  body: string;
  triggerDate: Date;
  workoutSessionId?: string;
}
