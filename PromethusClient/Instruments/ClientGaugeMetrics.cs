using Prometheus;
using PromethusClient.Settings;

namespace PromethusClient.Instruments
{
    public static class ClientGaugeMetrics
    {
        // it does not need to clear as it is updated with current state
        public static Gauge ConnectedClientsGauge { get; set; } = Metrics.CreateGauge(
                                                                                  "signalr_current_connected_clients_count",
                                                                                  "Number of connected clients",
                                                                                  new GaugeConfiguration { LabelNames = new[] { "signalr_current_connected_connection_ids" } }
                                                                              );

        // needs to reset on set time interval from app settings 
        //public static Gauge MessageSendForSetTimeSpanGauge { get; } = Metrics.CreateGauge(
        //                                                                          $"signalr_Mesage_Send_Per_{PromethusSettings.TimeSpanToResetSendMessagesCountInSeconds}_seconds",
        //                                                                          $"Number of Messages sent to clients in {PromethusSettings.TimeSpanToResetSendMessagesCountInSeconds} seconds",
        //                                                                          new GaugeConfiguration { LabelNames = new[] { "signalr_message_sent_to_connection_ids" } }
        //                                                                      );

        public static Gauge MessageSendForSetTimeSpanGauge { get; set; } = Metrics.CreateGauge(
                                                                                  $"signalr_Mesage_Send",
                                                                                  $"Number of Messages sent to clients",
                                                                                  new GaugeConfiguration { LabelNames = new[] { "signalr_message_sent_to_connection_ids" } }
                                                                              );

        public static Gauge UDPReceiveFameCount { get; set; } = Metrics.CreateGauge(
                                                                                  $"signalr_no_of_udp_packets_received",
                                                                                  $"Number of Udp packets received"
                                                                              );
        // it does not need to clear as it is updated with current state
        //public static Gauge CurrentAuthenticatedSessionsGauge { get; } = Metrics.CreateGauge(
        //                                                                          $"signalr_current_Authenticated_sessions",
        //                                                                          "Number of current Authenticated sessions",
        //                                                                          new GaugeConfiguration { LabelNames = new[] { "Remote_current_Authenticated_IPs" } }
        //                                                                      );

        // needs to reset on time interval from appsettings
        //public static Gauge TotalNotAuthenticatedSessionGauge { get; } = Metrics.CreateGauge(
        //                                                  $"signalr_Non_Authenticated_Session_Count_per_{PromethusSettings.TimeSpanToResetTotalNotAuthenticatedSessionsInSeconds}_seconds",
        //                                                  $"signalr Non Authenticated Session Count per_{PromethusSettings.TimeSpanToResetTotalNotAuthenticatedSessionsInSeconds}_seconds",
        //                                                  new GaugeConfiguration { LabelNames = new[] { "RemoteIPs" } }
        //                                                                  );
        #region doNotRequireReset

        public static Task<bool> UpdateConnectedClientsGauge(List<string> connectedConnectionIds)
        {
            try
            {
                if (connectedConnectionIds != null && connectedConnectionIds.Count() > 0)
                {
                    connectedConnectionIds = connectedConnectionIds.DistinctBy(x => x).ToList();
                    ConnectedClientsGauge.Set(connectedConnectionIds.Count());

                    if (PromethusSettings.IsAddConnectionIdsToMatrics)
                    {
                        //List<string> allCurrentConnections = new();
                        //var labelValues = ConnectedClientsGauge.GetAllLabelValues();
                        //if (labelValues != null && labelValues.Count() > 0)
                        //{
                        //    foreach (var value in labelValues)
                        //    {
                        //        if (value != null && value.Count() > 0)
                        //        {
                        //            foreach (var item in value)
                        //            {
                        //                if (!allCurrentConnections.Contains(item))
                        //                {
                        //                    allCurrentConnections.Add(item);
                        //                }
                        //            }
                        //        }
                        //    }
                        //}
                        //ConnectedClientsGauge.RemoveLabelled(string.Join(',', allCurrentConnections));
                        //foreach (var value in connectedConnectionIds)
                        //{
                        //    if (!allCurrentConnections.Contains(value))
                        //    {
                        //        allCurrentConnections.Add(value);
                        //    }
                        //}
                        ConnectedClientsGauge.WithLabels(string.Join(',', connectedConnectionIds));
                    }
                    return Task.FromResult(true);
                }
                else
                {
                    return Task.FromResult(false);

                }
            }
            catch (Exception ex)
            {
                ConnectedClientsGauge = Metrics.CreateGauge(
                                                                "signalr_current_connected_clients_count",
                                                                "Number of connected clients",
                                                                new GaugeConfiguration { LabelNames = new[] { "signalr_current_connected_connection_ids" } }
                                                            );
                return Task.FromResult(false);
            }
        }
        #region AuthenticationsessionSection
        //public static Task<bool> UpdateCurrentAuthenticatedSessions(List<string> currentAuthenticatedSessionIdsOrIPs, bool isAuthenticated)
        //{
        //    if (isAuthenticated && currentAuthenticatedSessionIdsOrIPs != null && currentAuthenticatedSessionIdsOrIPs.Count() > 0)
        //    {
        //        CurrentAuthenticatedSessionsGauge.Inc(currentAuthenticatedSessionIdsOrIPs.Count());
        //        if (PromethusSettings.IsAddConnectionIdsToMatrics)
        //        {
        //            List<string> allCurrentAuthenticatedSessionIdsOrIPs = new();
        //            var labelValues = CurrentAuthenticatedSessionsGauge.GetAllLabelValues();
        //            if (labelValues != null && labelValues.Count() > 0)
        //            {
        //                foreach (var value in labelValues)
        //                {
        //                    if (value != null && value.Count() > 0)
        //                    {
        //                        foreach (var ip in value)
        //                        {
        //                            if (!allCurrentAuthenticatedSessionIdsOrIPs.Contains(ip))
        //                            {
        //                                allCurrentAuthenticatedSessionIdsOrIPs.Add(ip);
        //                            }
        //                        }
        //                    }
        //                }
        //            }
        //            CurrentAuthenticatedSessionsGauge.RemoveLabelled(string.Join(',', allCurrentAuthenticatedSessionIdsOrIPs));
        //            foreach (var item in currentAuthenticatedSessionIdsOrIPs)
        //            {
        //                if (!allCurrentAuthenticatedSessionIdsOrIPs.Contains(item))
        //                {
        //                    allCurrentAuthenticatedSessionIdsOrIPs.Add(item);
        //                }
        //            }
        //            CurrentAuthenticatedSessionsGauge.WithLabels(string.Join(',', allCurrentAuthenticatedSessionIdsOrIPs));
        //        }
        //        return Task.FromResult(true);
        //    }
        //    else if (!isAuthenticated && currentAuthenticatedSessionIdsOrIPs != null && currentAuthenticatedSessionIdsOrIPs.Count() > 0)
        //    {
        //        CurrentAuthenticatedSessionsGauge.Dec(currentAuthenticatedSessionIdsOrIPs.Count());
        //        if (PromethusSettings.IsAddConnectionIdsToMatrics)
        //        {
        //            List<string> allCurrentAuthenticatedSessionIdsOrIPs = new();
        //            var labelValues = CurrentAuthenticatedSessionsGauge.GetAllLabelValues();
        //            if (currentAuthenticatedSessionIdsOrIPs != null && currentAuthenticatedSessionIdsOrIPs.Count() > 0)
        //            {
        //                foreach (var value in labelValues)
        //                {
        //                    if (value != null && value.Count() > 0)
        //                    {
        //                        foreach (var oldAuthIDs in value)
        //                        {
        //                            if (!allCurrentAuthenticatedSessionIdsOrIPs.Contains(oldAuthIDs))
        //                            {
        //                                allCurrentAuthenticatedSessionIdsOrIPs.Add(oldAuthIDs);
        //                            }
        //                        }
        //                    }
        //                }
        //                CurrentAuthenticatedSessionsGauge.RemoveLabelled(string.Join(',', allCurrentAuthenticatedSessionIdsOrIPs));

        //                foreach (var item in currentAuthenticatedSessionIdsOrIPs)
        //                {
        //                    if (allCurrentAuthenticatedSessionIdsOrIPs.Contains(item))
        //                    {
        //                        allCurrentAuthenticatedSessionIdsOrIPs.Remove(item);
        //                    }
        //                }
        //            }
        //            CurrentAuthenticatedSessionsGauge.WithLabels(string.Join(',', allCurrentAuthenticatedSessionIdsOrIPs));
        //        }
        //        return Task.FromResult(true);
        //    }
        //    else
        //    {
        //        return Task.FromResult(false);
        //    }
        //}

        #endregion AuthenticationsessionSection

        #endregion doNotRequireReset

        #region messageSentSection
        //public static Task<bool> UpdateUDPMessageCount(List<int> framesReceivedForIds)
        //{
        //    try
        //    {
        //        if (framesReceivedForIds != null && framesReceivedForIds.Count() > 0)
        //        {
        //            if ((UDPReceiveFameCount.Value + framesReceivedForIds.Count()) > 1.7976931348623157E+308 - 10000)
        //            {
        //                UDPReceiveFameCount.Set(0);
        //            }
        //            UDPReceiveFameCount.Inc(framesReceivedForIds.Count());
        //            return Task.FromResult(true);
        //        }
        //        return Task.FromResult(false);
        //    }
        //    catch (Exception)
        //    {
        //        return Task.FromResult(false);
        //    }
        //}
        public static Task<bool> UpdateUDPMessageCount(int count)
        {
            try
            {
                if (count > 0)
                {
                    if (UDPReceiveFameCount.Value > 1.7976931348623157E+308 - 10000)
                    {
                        UDPReceiveFameCount.Set(0);
                        UDPReceiveFameCount = Metrics.CreateGauge($"signalr_no_of_udp_packets_received",
                                                                   $"Number of Udp packets received"
                                                                  );
                    }
                    UDPReceiveFameCount.Inc(count);
                    return Task.FromResult(true);
                }
                return Task.FromResult(false);
            }
            catch (Exception)
            {
                UDPReceiveFameCount = Metrics.CreateGauge($"signalr_no_of_udp_packets_received",
                                                                   $"Number of Udp packets received"
                                                                  );
                return Task.FromResult(false);
            }
        }

        public static Task<bool> UpdateMessageSendForSetTimeSpan(List<string> messageSentToConnectionIds)
        {
            try
            {
                if (messageSentToConnectionIds != null && messageSentToConnectionIds.Count() > 0)
                {
                    if ((MessageSendForSetTimeSpanGauge.Value + messageSentToConnectionIds.Count()) > 1.7976931348623157E+308 - 10000)
                    {
                        MessageSendForSetTimeSpanGauge.Set(0);
                        MessageSendForSetTimeSpanGauge = Metrics.CreateGauge($"signalr_Mesage_Send",
                                                $"Number of Messages sent to clients",
                                                 new GaugeConfiguration { LabelNames = new[] { "signalr_message_sent_to_connection_ids" } }
                                              );
                    }
                    MessageSendForSetTimeSpanGauge.Inc(messageSentToConnectionIds.Count());
                    if (PromethusSettings.IsAddConnectionIdsToMatrics)
                    {
                        //List<string> allCoonectionIdsMessageSentInSetTimeSpan = new();
                        //var labelValues = MessageSendForSetTimeSpanGauge.GetAllLabelValues();
                        //if (labelValues != null && labelValues.Count() > 0)
                        //{
                        //    foreach (var value in labelValues)
                        //    {
                        //        if (value != null && value.Count() > 0)
                        //        {
                        //            foreach (var connectionIdold in value)
                        //            {
                        //                if (!allCoonectionIdsMessageSentInSetTimeSpan.Contains(connectionIdold))
                        //                {
                        //                    allCoonectionIdsMessageSentInSetTimeSpan.Add(connectionIdold);
                        //                }
                        //            }
                        //        }
                        //    }
                        //}

                        //MessageSendForSetTimeSpanGauge.RemoveLabelled(string.Join(',', allCoonectionIdsMessageSentInSetTimeSpan));

                        //foreach (var value in messageSentToConnectionIds)
                        //{
                        //    if (!allCoonectionIdsMessageSentInSetTimeSpan.Contains(value))
                        //    {
                        //        allCoonectionIdsMessageSentInSetTimeSpan.Add(value);
                        //    }
                        //}
                        MessageSendForSetTimeSpanGauge.WithLabels(string.Join(',', messageSentToConnectionIds));
                    }
                    return Task.FromResult(true);
                }
                else
                {
                    return Task.FromResult(false);
                }
            }
            catch (Exception ex)
            {
                MessageSendForSetTimeSpanGauge = Metrics.CreateGauge($"signalr_Mesage_Send",
                                                $"Number of Messages sent to clients",
                                                 new GaugeConfiguration { LabelNames = new[] { "signalr_message_sent_to_connection_ids" } });
                return Task.FromResult(false);
            }
        }
        //public static Task<bool> ReSetMessageSendForSetTimeSpan()
        //{
        //    MessageSendForSetTimeSpanGauge.Set(0);
        //    if (PromethusSettings.IsAddConnectionIdsToMatrics)
        //    {
        //        List<string> allCoonectionIdsMessageSentInSetTimeSpan = new();
        //        var labelValues = MessageSendForSetTimeSpanGauge.GetAllLabelValues();
        //        if (labelValues != null && labelValues.Count() > 0)
        //        {
        //            foreach (var value in labelValues)
        //            {
        //                if (value != null && value.Count() > 0)
        //                {
        //                    foreach (var connectionIdold in value)
        //                    {
        //                        if (!allCoonectionIdsMessageSentInSetTimeSpan.Contains(connectionIdold))
        //                        {
        //                            allCoonectionIdsMessageSentInSetTimeSpan.Add(connectionIdold);
        //                        }
        //                    }
        //                }
        //            }
        //        }
        //        MessageSendForSetTimeSpanGauge.RemoveLabelled(string.Join(',', allCoonectionIdsMessageSentInSetTimeSpan));
        //    }
        //    return Task.FromResult(true);
        //}
        #endregion messageSentSection



        #region NonAuthenticationSessionSection
        //public static Task<bool> UpdateTotalNotAuthenticatedSessionGauge(List<string> currentNonAuthenticatedSessionIdsOrIPs, bool isAuthenticated)
        //{
        //    try
        //    {
        //        if (!isAuthenticated && currentNonAuthenticatedSessionIdsOrIPs != null && currentNonAuthenticatedSessionIdsOrIPs.Count() > 0)
        //        {
        //            int count = currentNonAuthenticatedSessionIdsOrIPs.Count();
        //            TotalNotAuthenticatedSessionGauge.Inc(count);
        //            if (PromethusSettings.IsAddConnectionIdsToMatrics)
        //            {
        //                List<string> allCurrentNonAuthenticatedSessionIdsOrIPs = new();
        //                var labelValues = TotalNotAuthenticatedSessionGauge.GetAllLabelValues();
        //                if (labelValues != null && labelValues.Count() > 0)
        //                {
        //                    foreach (var value in labelValues)
        //                    {
        //                        if (value != null && value.Count() > 0)
        //                        {
        //                            foreach (var item in value)
        //                            {
        //                                if (!allCurrentNonAuthenticatedSessionIdsOrIPs.Contains(item))
        //                                {
        //                                    allCurrentNonAuthenticatedSessionIdsOrIPs.Add(item);
        //                                }
        //                            }
        //                        }
        //                    }
        //                }
        //                TotalNotAuthenticatedSessionGauge.RemoveLabelled(string.Join(',', allCurrentNonAuthenticatedSessionIdsOrIPs));

        //                foreach (var value in currentNonAuthenticatedSessionIdsOrIPs)
        //                {
        //                    if (!allCurrentNonAuthenticatedSessionIdsOrIPs.Contains(value))
        //                    {
        //                        allCurrentNonAuthenticatedSessionIdsOrIPs.Add(value);
        //                    }
        //                }
        //                TotalNotAuthenticatedSessionGauge.WithLabels(string.Join(',', allCurrentNonAuthenticatedSessionIdsOrIPs));
        //            }
        //            return Task.FromResult(true);
        //        }
        //        else if (isAuthenticated && currentNonAuthenticatedSessionIdsOrIPs != null && currentNonAuthenticatedSessionIdsOrIPs.Count() > 0)
        //        {
        //            TotalNotAuthenticatedSessionGauge.Dec(currentNonAuthenticatedSessionIdsOrIPs.Count());
        //            if (PromethusSettings.IsAddConnectionIdsToMatrics)
        //            {
        //                List<string> allCurrentNonAuthenticatedSessionIdsOrIPsOLD = new();
        //                List<string> allCurrentNonAuthenticatedSessionIdsOrIPs = new();
        //                var labelValues = TotalNotAuthenticatedSessionGauge.GetAllLabelValues();
        //                if (labelValues != null && labelValues.Count() > 0)
        //                {
        //                    foreach (var value in labelValues)
        //                    {
        //                        if (value != null && value.Count() > 0)
        //                        {
        //                            foreach (var oldAuthIDs in value)
        //                            {
        //                                if (allCurrentNonAuthenticatedSessionIdsOrIPs.Contains(oldAuthIDs))
        //                                {
        //                                    allCurrentNonAuthenticatedSessionIdsOrIPs.Remove(oldAuthIDs);
        //                                }
        //                                allCurrentNonAuthenticatedSessionIdsOrIPsOLD.Add(oldAuthIDs);
        //                            }
        //                        }
        //                    }
        //                }
        //                TotalNotAuthenticatedSessionGauge.RemoveLabelled(string.Join(',', allCurrentNonAuthenticatedSessionIdsOrIPsOLD));
        //                foreach (var item in currentNonAuthenticatedSessionIdsOrIPs)
        //                {
        //                    if (!allCurrentNonAuthenticatedSessionIdsOrIPs.Contains(item))
        //                    {
        //                        allCurrentNonAuthenticatedSessionIdsOrIPs.Add(item);
        //                    }
        //                }
        //                TotalNotAuthenticatedSessionGauge.WithLabels(string.Join(',', allCurrentNonAuthenticatedSessionIdsOrIPs));
        //            }
        //            return Task.FromResult(true);
        //        }
        //        else
        //        {
        //            return Task.FromResult(false);
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        return Task.FromResult(true);
        //    }
        //}
        //public static Task<bool> ReSetTotalNotAuthenticatedSessionGauge()
        //{
        //    TotalNotAuthenticatedSessionGauge.Set(0);
        //    if (PromethusSettings.IsAddConnectionIdsToMatrics)
        //    {
        //        List<string> allCurrentNonAuthenticatedSessionIdsOrIPs = new();
        //        var labelValues = TotalNotAuthenticatedSessionGauge.GetAllLabelValues();
        //        if (labelValues != null && labelValues.Count() > 0)
        //        {
        //            foreach (var value in labelValues)
        //            {
        //                if (value != null && value.Count() > 0)
        //                {
        //                    foreach (var oldAuthIDs in value)
        //                    {
        //                        allCurrentNonAuthenticatedSessionIdsOrIPs.Add(oldAuthIDs);
        //                    }
        //                }
        //            }
        //        }
        //        TotalNotAuthenticatedSessionGauge.RemoveLabelled(string.Join(',', allCurrentNonAuthenticatedSessionIdsOrIPs));
        //    }
        //    return Task.FromResult(true);
        //}
        #endregion NonAuthenticationSessionSection
    }
}